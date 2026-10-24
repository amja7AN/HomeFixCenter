using System;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Devices
{
    // ثلاجة: فئة فرعية من Device.
    // التكلفة = (رسم الفحص + قطع الغيار + ثمن الغاز المستعمل + كلفة الضاغط إن استبدل)
    // وتضرب في 1.2 إذا كانت الثلاجة نوع نو فروست (لأن قطعها الإلكترونية أغلى)
    public class Fridge : Device
    {
        public const int MinLiters = 50;
        public const int MaxLiters = 800;
        public const decimal MaxGasKg = 5m;

        private const decimal InspectionFee = 15m;
        private const decimal CompressorFee = 90m;
        private const decimal GasPricePerKg = 14m;
        private const decimal NoFrostRate = 1.2m;

        // صفات الثلاجة: لا تتغير بعد الإنشاء
        private readonly int _volumeLiters;
        private readonly bool _isNoFrost;

        // أعمال الإصلاح: تسجل أثناء الصيانة فقط
        private bool _compressorReplaced;
        private decimal _gasUsedKg;

        public Fridge(string serialCode, string manufacturer, string customerName, string faultDescription,
                      DateTime receivedOn, DateTime dueDate, bool underWarranty,
                      int volumeLiters, bool isNoFrost)
            : base(serialCode, manufacturer, customerName, faultDescription, receivedOn, dueDate, underWarranty)
        {
            if (!DataValidator.IsBetween(volumeLiters, MinLiters, MaxLiters))
            {
                throw new ArgumentException("Fridge capacity must be between " + MinLiters + " and " + MaxLiters + " liters.");
            }

            _volumeLiters = volumeLiters;
            _isNoFrost = isNoFrost;
        }

        public int VolumeLiters
        {
            get { return _volumeLiters; }
        }

        public bool IsNoFrost
        {
            get { return _isNoFrost; }
        }

        public bool CompressorReplaced
        {
            get { return _compressorReplaced; }
            set
            {
                EnsureInMaintenance("record the compressor work");
                _compressorReplaced = value;
            }
        }

        public decimal GasUsedKg
        {
            get { return _gasUsedKg; }
            set
            {
                EnsureInMaintenance("record the gas used");

                if (!DataValidator.IsBetween(value, 0m, MaxGasKg))
                {
                    throw new ArgumentException("Gas used must be between 0 and " + MaxGasKg + " kg.");
                }

                _gasUsedKg = value;
            }
        }

        public override string TypeName
        {
            get { return "Fridge"; }
        }

        // تعريف الدالة المجردة بطريقة خاصة بالثلاجة
        public override decimal ComputeRepairCost()
        {
            decimal sum = InspectionFee + SparePartsCost + (GasUsedKg * GasPricePerKg);

            if (CompressorReplaced)
            {
                sum += CompressorFee;
            }

            if (IsNoFrost)
            {
                sum = sum * NoFrostRate;
            }

            return ApplyWarranty(sum);
        }

        protected override string GetSpecificDetails()
        {
            return "Capacity: " + VolumeLiters + " L | No-frost: " + OutputFormat.ToYesNo(IsNoFrost)
                   + " | Compressor replaced: " + OutputFormat.ToYesNo(CompressorReplaced)
                   + " | Gas used: " + GasUsedKg + " kg";
        }
    }
}
