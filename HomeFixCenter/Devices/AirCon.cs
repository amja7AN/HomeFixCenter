using System;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Devices
{
    // مكيف: فئة فرعية من Device.
    // التكلفة = رسم الفحص + قطع الغيار + (طول الأنابيب بالمتر × سعر المتر)
    //           + رسم الزيارة المنزلية إن لزمت + رسم فحص البطاقة إذا كان المكيف إنفرتر
    public class AirCon : Device
    {
        public const int MinBtu = 9000;
        public const int MaxBtu = 60000;
        public const decimal MaxPipeMeters = 30m;

        private const decimal InspectionFee = 20m;
        private const decimal PipeFeePerMeter = 6m;
        private const decimal HomeVisitFee = 25m;
        private const decimal InverterBoardFee = 18m;

        // صفات المكيف: لا تتغير بعد الإنشاء
        private readonly int _coolingBtu;
        private readonly bool _hasInverter;

        // أعمال الإصلاح: تسجل أثناء الصيانة فقط
        private bool _homeVisit;
        private decimal _pipeLengthMeters;

        public AirCon(string serialCode, string manufacturer, string customerName, string faultDescription,
                      DateTime receivedOn, DateTime dueDate, bool underWarranty,
                      int coolingBtu, bool hasInverter)
            : base(serialCode, manufacturer, customerName, faultDescription, receivedOn, dueDate, underWarranty)
        {
            if (!DataValidator.IsBetween(coolingBtu, MinBtu, MaxBtu))
            {
                throw new ArgumentException("Cooling capacity must be between " + MinBtu + " and " + MaxBtu + " BTU.");
            }

            _coolingBtu = coolingBtu;
            _hasInverter = hasInverter;
        }

        public int CoolingBtu
        {
            get { return _coolingBtu; }
        }

        public bool HasInverter
        {
            get { return _hasInverter; }
        }

        public bool HomeVisit
        {
            get { return _homeVisit; }
            set
            {
                EnsureInMaintenance("record the home visit");
                _homeVisit = value;
            }
        }

        public decimal PipeLengthMeters
        {
            get { return _pipeLengthMeters; }
            set
            {
                EnsureInMaintenance("record the pipe length");

                if (!DataValidator.IsBetween(value, 0m, MaxPipeMeters))
                {
                    throw new ArgumentException("Pipe length must be between 0 and " + MaxPipeMeters + " meters.");
                }

                _pipeLengthMeters = value;
            }
        }

        public override string TypeName
        {
            get { return "Air conditioner"; }
        }

        // تعريف الدالة المجردة بطريقة خاصة بالمكيف
        public override decimal ComputeRepairCost()
        {
            decimal sum = InspectionFee + SparePartsCost + (PipeLengthMeters * PipeFeePerMeter);

            if (HomeVisit)
            {
                sum += HomeVisitFee;
            }

            if (HasInverter)
            {
                sum += InverterBoardFee;
            }

            return ApplyWarranty(sum);
        }

        protected override string GetSpecificDetails()
        {
            return "Cooling: " + CoolingBtu + " BTU | Inverter: " + OutputFormat.ToYesNo(HasInverter)
                   + " | Home visit: " + OutputFormat.ToYesNo(HomeVisit)
                   + " | Pipes: " + PipeLengthMeters + " m";
        }
    }
}
