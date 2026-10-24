using System;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Devices
{
    // غسالة: فئة فرعية من Device.
    // التكلفة = رسم الفحص + (سعة الغسالة بالكيلو × رسم المناولة) + قطع الغيار
    //           + كلفة المحرك إن استبدل + كلفة المضخة إن استبدلت
    // وتضرب في 1.1 إذا كانت الغسالة أمامية التحميل (لأن فكها أصعب)
    public class Washer : Device
    {
        public const int MinKg = 3;
        public const int MaxKg = 20;

        private const decimal InspectionFee = 10m;
        private const decimal HandlingFeePerKg = 2m;
        private const decimal MotorFee = 70m;
        private const decimal PumpFee = 25m;
        private const decimal FrontLoadRate = 1.1m;

        // صفات الغسالة: لا تتغير بعد الإنشاء
        private readonly int _drumKg;
        private readonly bool _hasFrontDoor;

        // أعمال الإصلاح: تسجل أثناء الصيانة فقط
        private bool _motorReplaced;
        private bool _pumpReplaced;

        public Washer(string serialCode, string manufacturer, string customerName, string faultDescription,
                      DateTime receivedOn, DateTime dueDate, bool underWarranty,
                      int drumKg, bool hasFrontDoor)
            : base(serialCode, manufacturer, customerName, faultDescription, receivedOn, dueDate, underWarranty)
        {
            if (!DataValidator.IsBetween(drumKg, MinKg, MaxKg))
            {
                throw new ArgumentException("Washer load capacity must be between " + MinKg + " and " + MaxKg + " kg.");
            }

            _drumKg = drumKg;
            _hasFrontDoor = hasFrontDoor;
        }

        public int DrumKg
        {
            get { return _drumKg; }
        }

        public bool HasFrontDoor
        {
            get { return _hasFrontDoor; }
        }

        public bool MotorReplaced
        {
            get { return _motorReplaced; }
            set
            {
                EnsureInMaintenance("record the motor work");
                _motorReplaced = value;
            }
        }

        public bool PumpReplaced
        {
            get { return _pumpReplaced; }
            set
            {
                EnsureInMaintenance("record the pump work");
                _pumpReplaced = value;
            }
        }

        public override string TypeName
        {
            get { return "Washer"; }
        }

        // تعريف الدالة المجردة بطريقة خاصة بالغسالة
        public override decimal ComputeRepairCost()
        {
            decimal sum = InspectionFee + (DrumKg * HandlingFeePerKg) + SparePartsCost;

            if (MotorReplaced)
            {
                sum += MotorFee;
            }

            if (PumpReplaced)
            {
                sum += PumpFee;
            }

            if (HasFrontDoor)
            {
                sum = sum * FrontLoadRate;
            }

            return ApplyWarranty(sum);
        }

        protected override string GetSpecificDetails()
        {
            return "Load: " + DrumKg + " kg | Front-load: " + OutputFormat.ToYesNo(HasFrontDoor)
                   + " | Motor replaced: " + OutputFormat.ToYesNo(MotorReplaced)
                   + " | Pump replaced: " + OutputFormat.ToYesNo(PumpReplaced);
        }
    }
}
