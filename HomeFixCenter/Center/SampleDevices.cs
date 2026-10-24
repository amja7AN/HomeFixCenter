using System;
using HomeFixCenter.Devices;

namespace HomeFixCenter.Center
{
    // فئة ثابتة تضيف أجهزة تجريبية بمراحل مختلفة لنجرب البرنامج بسرعة.
    // نغير مراحل الأجهزة قبل إضافتها للمركز حتى لا تظهر إشعارات أثناء التحميل
    public static class SampleDevices
    {
        // تضيف الأجهزة وتعيد عددها
        public static int Fill(ServiceCenter serviceCenter)
        {
            DateTime asOf = DateTime.Today;

            // 1) ثلاجة قيد الصيانة ومتأخرة عن موعدها
            Fridge fridge1 = new Fridge("RF-2026-001", "Beko", "Hassan Darwish", "Not cooling properly",
                                        asOf.AddDays(-10), asOf.AddDays(-2), false, 350, true);
            fridge1.StartMaintenance();
            fridge1.CompressorReplaced = true;
            fridge1.GasUsedKg = 0.5m;
            fridge1.SparePartsCost = 35m;

            // 2) ثلاجة مشمولة بالكفالة، تم إصلاحها وجاهزة للاستلام
            Fridge fridge2 = new Fridge("RF-2026-002", "Haier", "Maya Ibrahim", "Door seal is torn",
                                        asOf.AddDays(-14), asOf.AddDays(-9), true, 280, false);
            fridge2.StartMaintenance();
            fridge2.SparePartsCost = 12m;
            fridge2.FinishMaintenance();

            // 3) غسالة أمامية قيد الصيانة
            Washer washer1 = new Washer("WM-2026-001", "Zanussi", "Karim Suleiman", "Drum does not spin",
                                        asOf.AddDays(-6), asOf.AddDays(3), false, 7, true);
            washer1.StartMaintenance();
            washer1.MotorReplaced = true;
            washer1.SparePartsCost = 20m;

            // 4) غسالة تم إصلاحها وتسليمها
            Washer washer2 = new Washer("WM-2026-002", "Hitachi", "Nour Haddad", "Water does not drain",
                                        asOf.AddDays(-18), asOf.AddDays(-11), false, 9, false);
            washer2.StartMaintenance();
            washer2.PumpReplaced = true;
            washer2.SparePartsCost = 15m;
            washer2.FinishMaintenance();
            washer2.HandOver();

            // 5) مكيف مسجل وبانتظار الفني
            AirCon airCon1 = new AirCon("AC-2026-001", "Carrier", "Omar Khoury", "Water leaks from the indoor unit",
                                        asOf.AddDays(-4), asOf.AddDays(6), false, 18000, false);

            // 6) مكيف إنفرتر كبير قيد الصيانة ويحتاج زيارة منزلية
            AirCon airCon2 = new AirCon("AC-2026-002", "Daikin", "Rana Mansour", "Not cooling at all",
                                        asOf.AddDays(-3), asOf.AddDays(5), false, 24000, true);
            airCon2.StartMaintenance();
            airCon2.HomeVisit = true;
            airCon2.PipeLengthMeters = 4m;
            airCon2.SparePartsCost = 40m;

            // تعدد الأشكال: نضع الأنواع الثلاثة في مصفوفة واحدة من نوع Device
            Device[] all = { fridge1, fridge2, washer1, washer2, airCon1, airCon2 };

            foreach (Device device in all)
            {
                serviceCenter.AddDevice(device);
            }

            return all.Length;
        }
    }
}
