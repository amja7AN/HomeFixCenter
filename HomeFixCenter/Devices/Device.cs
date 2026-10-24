using System;
using System.Text;
using HomeFixCenter.Alerts;
using HomeFixCenter.Contracts;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Devices
{
    // الفئة المجردة (Abstract Class): تمثل أي جهاز منزلي داخل المركز وتطبق الواجهة IMaintainable.
    // فيها كل ما هو مشترك بين الأجهزة، وتترك حساب التكلفة للفئات الفرعية لأن لكل جهاز طريقته
    public abstract class Device : IMaintainable
    {
        // ثوابت عامة
        public const decimal MaxSparePartsCost = 5000m;     // أعلى تكلفة مسموحة لقطع الغيار
        public const decimal WarrantyDiscountRate = 0.5m;    // الجهاز المشمول بالكفالة يدفع نصف التكلفة فقط

        // ---------- حقول ثابتة (static) مشتركة بين كل الأجهزة، تستخدم في الإحصائيات ----------
        private static int _totalDevices;
        private static int _inMaintenanceCount;
        private static int _repairedCount;
        private static int _handedOverCount;
        private static decimal _totalRevenue;

        // ---------- حقول خاصة (Encapsulation) ----------
        // حقول لا تتغير بعد إنشاء الكائن
        private readonly string _serialCode;
        private readonly string _manufacturer;
        private readonly DateTime _receivedOn;
        private readonly bool _underWarranty;

        // حقول يمكن تغييرها لكن عن طريق خصائص تتحقق من القيمة
        private string _customerName;
        private string _faultDescription;
        private DateTime _dueDate;
        private decimal _sparePartsCost;
        private MaintenanceStage _stage;
        private bool _overdueReported;   // حتى لا نكرر إشعار التأخير لنفس الموعد

        // ---------- الأحداث (Events): مبنية على التفويضات المعرفة في مجلد Alerts ----------

        // يطلق عند تغير مرحلة الجهاز (ومنها اكتمال الإصلاح)
        public event StageChangedHandler StageChanged;

        // يطلق عند تأخر تسليم الجهاز عن موعده
        public event OverdueHandler DeliveryOverdue;

        // المنشئ: يتحقق من كل القيم قبل إنشاء الجهاز ويرمي ArgumentException إذا كانت خاطئة
        protected Device(string serialCode, string manufacturer, string customerName,
                         string faultDescription, DateTime receivedOn, DateTime dueDate, bool underWarranty)
        {
            if (!DataValidator.IsSerialValid(serialCode))
            {
                throw new ArgumentException("Invalid serial number. Use 2 letters, a dash, 4 digits, a dash, then 3 digits (example RF-2026-014).");
            }

            if (!DataValidator.IsTextValid(manufacturer, 2, 30))
            {
                throw new ArgumentException("Manufacturer must be between 2 and 30 characters.");
            }

            if (!DataValidator.IsReceivedDateValid(receivedOn))
            {
                throw new ArgumentException("Received date cannot be in the future or older than 5 years.");
            }

            _serialCode = serialCode.Trim().ToUpper();
            _manufacturer = manufacturer.Trim();
            _receivedOn = receivedOn.Date;
            _underWarranty = underWarranty;
            _stage = MaintenanceStage.Registered;

            // نمرر القيم الباقية عبر الخصائص لتطبق عليها قواعد التحقق نفسها
            CustomerName = customerName;
            FaultDescription = faultDescription;
            DueDate = dueDate;

            _totalDevices++;
        }

        // ---------- خصائص للقراءة فقط بعد الإنشاء (لا يوجد set) ----------

        // الرقم التسلسلي لا يتغير أبدا
        public string SerialCode
        {
            get { return _serialCode; }
        }

        public string Manufacturer
        {
            get { return _manufacturer; }
        }

        public DateTime ReceivedOn
        {
            get { return _receivedOn; }
        }

        public bool UnderWarranty
        {
            get { return _underWarranty; }
        }

        // المرحلة الحالية تتغير فقط عبر StartMaintenance و FinishMaintenance و HandOver
        public MaintenanceStage Stage
        {
            get { return _stage; }
        }

        // هل يمكن تسجيل أعمال الإصلاح الآن؟ (فقط أثناء الصيانة)
        public bool CanRecordWork
        {
            get { return _stage == MaintenanceStage.InMaintenance; }
        }

        // ---------- خصائص قابلة للتعديل مع التحقق ----------

        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                if (!DataValidator.IsPersonNameValid(value))
                {
                    throw new ArgumentException("Customer name must be 2-40 characters: letters, spaces, dots or dashes only.");
                }

                _customerName = value.Trim();
            }
        }

        public string FaultDescription
        {
            get { return _faultDescription; }
            set
            {
                if (!DataValidator.IsTextValid(value, 3, 150))
                {
                    throw new ArgumentException("Fault description must be between 3 and 150 characters.");
                }

                _faultDescription = value.Trim();
            }
        }

        // موعد التسليم: يمكن تأجيله لكن لا يسبق تاريخ الاستلام، ولا يتغير بعد انتهاء الإصلاح
        public DateTime DueDate
        {
            get { return _dueDate; }
            set
            {
                if (_stage == MaintenanceStage.Repaired || _stage == MaintenanceStage.HandedOver)
                {
                    throw new InvalidOperationException("The due date cannot be changed after the repair is finished.");
                }

                if (!DataValidator.IsDueDateValid(_receivedOn, value))
                {
                    throw new ArgumentException("The due date cannot be earlier than the received date.");
                }

                _dueDate = value.Date;
                _overdueReported = false;   // الموعد تغير، فيجوز إشعار تأخير جديد
            }
        }

        // تكلفة قطع الغيار: تسجل فقط أثناء الصيانة
        public decimal SparePartsCost
        {
            get { return _sparePartsCost; }
            set
            {
                EnsureInMaintenance("record the parts cost");

                if (!DataValidator.IsBetween(value, 0m, MaxSparePartsCost))
                {
                    throw new ArgumentException("Parts cost must be between 0 and " + MaxSparePartsCost + ".");
                }

                _sparePartsCost = value;
            }
        }

        // ---------- الأعضاء المجردة: كل فئة فرعية يجب أن تعرفها بطريقتها ----------

        // اسم نوع الجهاز للعرض
        public abstract string TypeName { get; }

        // دالة مجردة: حساب تكلفة الإصلاح، وهي تنفيذ للدالة المعرفة في الواجهة
        public abstract decimal ComputeRepairCost();

        // دالة مجردة: تفاصيل خاصة بكل نوع جهاز، تضاف إلى الملخص
        protected abstract string GetSpecificDetails();

        // ---------- العمليات المشتركة: تنقل الجهاز بين المراحل وتطلق الأحداث ----------

        public void StartMaintenance()
        {
            RequireStage(MaintenanceStage.Registered, "start maintenance");
            _inMaintenanceCount++;
            MoveTo(MaintenanceStage.InMaintenance);
        }

        public void FinishMaintenance()
        {
            RequireStage(MaintenanceStage.InMaintenance, "finish maintenance");
            _inMaintenanceCount--;
            _repairedCount++;
            MoveTo(MaintenanceStage.Repaired);
        }

        public void HandOver()
        {
            RequireStage(MaintenanceStage.Repaired, "hand over the device");
            _handedOverCount++;
            _totalRevenue += ComputeRepairCost();   // الإيراد يحسب عند التسليم
            MoveTo(MaintenanceStage.HandedOver);
        }

        // ---------- التأخير ----------

        // هل الجهاز متأخر في التاريخ المعطى؟ (ما زال في المركز ولم يصلح بعد)
        public bool IsOverdue(DateTime asOf)
        {
            bool jobOpen = _stage == MaintenanceStage.Registered || _stage == MaintenanceStage.InMaintenance;
            return jobOpen && asOf.Date > _dueDate;
        }

        // يفحص التأخير ويطلق الحدث DeliveryOverdue مرة واحدة فقط لكل موعد
        public void CheckDueDate(DateTime asOf)
        {
            if (_overdueReported || !IsOverdue(asOf))
            {
                return;
            }

            _overdueReported = true;
            int lateDays = (asOf.Date - _dueDate).Days;

            if (DeliveryOverdue != null)
            {
                DeliveryOverdue(this, lateDays);
            }
        }

        // ---------- العرض ----------

        // ملخص الجهاز: الجزء المشترك هنا، والجزء الخاص من الفئة الفرعية (تعدد الأشكال)
        public string GetSummary()
        {
            StringBuilder content = new StringBuilder();
            content.AppendLine("[" + TypeName + "] " + SerialCode + "   Stage: " + Stage);
            content.AppendLine("   Manufacturer : " + Manufacturer + "   Customer : " + CustomerName);
            content.AppendLine("   Fault        : " + FaultDescription);
            content.AppendLine("   Received     : " + OutputFormat.Day(ReceivedOn) + "   Due : " + OutputFormat.Day(DueDate)
                            + "   Warranty : " + OutputFormat.ToYesNo(UnderWarranty));
            content.AppendLine("   Parts cost   : " + OutputFormat.Price(SparePartsCost) + "   Repair cost : " + OutputFormat.Price(ComputeRepairCost()));
            content.Append("   " + GetSpecificDetails());
            return content.ToString();
        }

        public override string ToString()
        {
            return TypeName + " " + SerialCode;
        }

        // ---------- خصائص ثابتة (static) للإحصائيات ----------

        // عدد كل الأجهزة التي سجلت
        public static int TotalDevices
        {
            get { return _totalDevices; }
        }

        // عدد الأجهزة قيد الصيانة الآن
        public static int InMaintenanceCount
        {
            get { return _inMaintenanceCount; }
        }

        // عدد الأجهزة التي تم إصلاحها (جاهزة أو مسلمة)
        public static int RepairedCount
        {
            get { return _repairedCount; }
        }

        // عدد الأجهزة التي سلمت لأصحابها
        public static int HandedOverCount
        {
            get { return _handedOverCount; }
        }

        // عدد الأجهزة المسجلة التي لم تبدأ صيانتها بعد (تحسب من الأعداد الأخرى)
        public static int WaitingCount
        {
            get { return _totalDevices - _inMaintenanceCount - _repairedCount; }
        }

        // مجموع ما تقاضاه المركز من الأجهزة المسلمة
        public static decimal TotalRevenue
        {
            get { return _totalRevenue; }
        }

        // ---------- دوال مساعدة للفئات الفرعية ----------

        // تطبق خصم الكفالة على المبلغ وتقرب الناتج لمنزلتين عشريتين
        protected decimal ApplyWarranty(decimal sum)
        {
            if (_underWarranty)
            {
                sum = sum * (1m - WarrantyDiscountRate);
            }

            return Math.Round(sum, 2);
        }

        // ترمي خطأ إذا كان الجهاز ليس قيد الصيانة (تستعملها الخصائص التي تسجل أعمال الإصلاح)
        protected void EnsureInMaintenance(string attempt)
        {
            if (_stage != MaintenanceStage.InMaintenance)
            {
                throw new InvalidOperationException("Cannot " + attempt + ": device " + _serialCode
                                                    + " must be in maintenance (current stage: " + _stage + ").");
            }
        }

        // ---------- دوال خاصة داخلية ----------

        private void RequireStage(MaintenanceStage neededStage, string attempt)
        {
            if (_stage != neededStage)
            {
                throw new InvalidOperationException("Cannot " + attempt + ": device " + _serialCode
                                                    + " is '" + _stage + "' but must be '" + neededStage + "'.");
            }
        }

        // تغير المرحلة ثم تعلم المشتركين بالحدث
        private void MoveTo(MaintenanceStage newStage)
        {
            MaintenanceStage oldStage = _stage;
            _stage = newStage;

            if (StageChanged != null)
            {
                StageChanged(this, oldStage, newStage);
            }
        }
    }
}
