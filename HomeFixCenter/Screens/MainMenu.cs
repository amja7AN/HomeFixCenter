using System;
using System.Collections.Generic;
using HomeFixCenter.Center;
using HomeFixCenter.Contracts;
using HomeFixCenter.Devices;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Screens
{
    // شاشة القائمة الرئيسية: تعرض الخيارات وتنفذ اختيار المستخدم.
    // لا تحتوي قواعد الصيانة نفسها (هي في الأجهزة)، فقط تسأل وتعرض النتائج
    public class MainMenu
    {
        private readonly ServiceCenter _serviceCenter;

        public MainMenu(ServiceCenter serviceCenter)
        {
            if (serviceCenter == null)
            {
                throw new ArgumentNullException("center");
            }

            _serviceCenter = serviceCenter;
        }

        // الحلقة الرئيسية: تعرض القائمة وتنفذ الخيار حتى يختار المستخدم الخروج
        public void Start()
        {
            bool exitRequested = false;

            while (!exitRequested)
            {
                ShowOptions();
                string selected = ConsoleInput.AskLine("Your choice: ");
                Console.WriteLine();

                try
                {
                    exitRequested = HandleChoice(selected);
                }
                catch (ArgumentException error)
                {
                    // خطأ في قيمة أدخلها المستخدم: نعرض رسالة مفهومة بدل إيقاف البرنامج
                    ConsoleInput.ShowError(error.Message);
                }
                catch (InvalidOperationException error)
                {
                    // عملية غير مسموحة في مرحلة الجهاز الحالية
                    ConsoleInput.ShowError(error.Message);
                }

                Console.WriteLine();
            }

            Console.WriteLine("Thank you for using HomeFix. Goodbye!");
        }

        private void ShowOptions()
        {
            Console.WriteLine("==================================================");
            Console.WriteLine("  " + _serviceCenter.CenterName);
            Console.WriteLine("==================================================");
            Console.WriteLine("   1. Register a new device");
            Console.WriteLine("   2. List all devices");
            Console.WriteLine("   3. Find a device by serial number");
            Console.WriteLine("   4. Start maintenance");
            Console.WriteLine("   5. Record repair work");
            Console.WriteLine("   6. Finish maintenance");
            Console.WriteLine("   7. Hand a device over to its customer");
            Console.WriteLine("   8. Postpone a due date");
            Console.WriteLine("   9. Devices still in maintenance");
            Console.WriteLine("  10. Repaired devices");
            Console.WriteLine("  11. Check overdue deliveries");
            Console.WriteLine("  12. Statistics");
            Console.WriteLine("  13. Notification log");
            Console.WriteLine("   0. Exit");
            Console.WriteLine("--------------------------------------------------");
        }

        // ينفذ الخيار المختار، ويعيد true فقط عند اختيار الخروج
        private bool HandleChoice(string selected)
        {
            switch (selected)
            {
                case "1":
                    RegisterDevice();
                    break;
                case "2":
                    PrintTable("All devices", _serviceCenter.GetAllDevices());
                    break;
                case "3":
                    FindDevice();
                    break;
                case "4":
                    StartMaintenance();
                    break;
                case "5":
                    RecordRepairWork();
                    break;
                case "6":
                    FinishMaintenance();
                    break;
                case "7":
                    HandOverDevice();
                    break;
                case "8":
                    PostponeDueDate();
                    break;
                case "9":
                    PrintTable("Devices still in maintenance", _serviceCenter.GetByStage(MaintenanceStage.InMaintenance));
                    break;
                case "10":
                    PrintTable("Repaired devices (ready or handed over)", _serviceCenter.GetRepairedDevices());
                    break;
                case "11":
                    CheckOverdue();
                    break;
                case "12":
                    ShowCenterNumbers();
                    break;
                case "13":
                    ShowNotificationLog();
                    break;
                case "0":
                    return true;
                default:
                    ConsoleInput.ShowError("That number is not in the menu. Please try again.");
                    break;
            }

            return false;
        }

        // ---------- تسجيل جهاز جديد ----------

        private void RegisterDevice()
        {
            Console.WriteLine("Device type:  1) Fridge    2) Washer    3) Air conditioner");
            int kindNumber = ConsoleInput.AskInt("Choose a type (1-3): ", 1, 3);

            // البيانات المشتركة بين كل الأنواع
            string serialCode = AskForNewSerial();
            string manufacturer = ConsoleInput.AskText("Manufacturer: ", 2, 30);
            string customer = ConsoleInput.AskPersonName("Customer name: ");
            string fault = ConsoleInput.AskText("Fault description: ", 3, 150);
            int repairDays = ConsoleInput.AskInt("Estimated repair time in days (1-90): ", 1, 90);
            bool covered = ConsoleInput.AskYesNo("Is the device under warranty? (y/n): ");

            DateTime arrivalDay = DateTime.Today;
            DateTime promisedDay = arrivalDay.AddDays(repairDays);

            // البيانات الخاصة بكل نوع، ثم إنشاء الكائن المناسب
            Device device;

            if (kindNumber == 1)
            {
                int liters = ConsoleInput.AskInt("Capacity in liters (" + Fridge.MinLiters + "-" + Fridge.MaxLiters + "): ",
                                                  Fridge.MinLiters, Fridge.MaxLiters);
                bool noFrost = ConsoleInput.AskYesNo("No-frost model? (y/n): ");
                device = new Fridge(serialCode, manufacturer, customer, fault, arrivalDay, promisedDay, covered, liters, noFrost);
            }
            else if (kindNumber == 2)
            {
                int kg = ConsoleInput.AskInt("Load capacity in kg (" + Washer.MinKg + "-" + Washer.MaxKg + "): ",
                                              Washer.MinKg, Washer.MaxKg);
                bool frontDoor = ConsoleInput.AskYesNo("Front-load model? (y/n): ");
                device = new Washer(serialCode, manufacturer, customer, fault, arrivalDay, promisedDay, covered, kg, frontDoor);
            }
            else
            {
                int btuValue = ConsoleInput.AskInt("Cooling capacity in BTU (" + AirCon.MinBtu + "-" + AirCon.MaxBtu + "): ",
                                               AirCon.MinBtu, AirCon.MaxBtu);
                bool withInverter = ConsoleInput.AskYesNo("Inverter model? (y/n): ");
                device = new AirCon(serialCode, manufacturer, customer, fault, arrivalDay, promisedDay, covered, btuValue, withInverter);
            }

            _serviceCenter.AddDevice(device);
            ConsoleInput.ShowSuccess("Device registered successfully:");
            Console.WriteLine(device.GetSummary());
        }

        // تسأل عن رقم تسلسلي صحيح وغير مستعمل من قبل
        private string AskForNewSerial()
        {
            while (true)
            {
                string serialCode = ConsoleInput.AskLine("Serial number (example RF-2026-014): ");

                if (!DataValidator.IsSerialValid(serialCode))
                {
                    ConsoleInput.ShowError("Use 2 letters, a dash, 4 digits, a dash, then 3 digits.");
                }
                else if (_serviceCenter.GetBySerial(serialCode) != null)
                {
                    ConsoleInput.ShowError("This serial number is already registered.");
                }
                else
                {
                    return serialCode;
                }
            }
        }

        // ---------- العمليات على جهاز موجود ----------

        // تسأل عن الرقم التسلسلي وتعيد الجهاز، أو null (مع رسالة خطأ) إذا لم تجده
        private Device AskForDevice()
        {
            string serialCode = ConsoleInput.AskLine("Enter the serial number: ");
            Device device = _serviceCenter.GetBySerial(serialCode);

            if (device == null)
            {
                ConsoleInput.ShowError("No device found with serial number '" + serialCode + "'.");
            }

            return device;
        }

        private void FindDevice()
        {
            Device device = AskForDevice();

            if (device != null)
            {
                Console.WriteLine(device.GetSummary());
            }
        }

        private void StartMaintenance()
        {
            Device device = AskForDevice();

            if (device != null)
            {
                device.StartMaintenance();
            }
        }

        private void FinishMaintenance()
        {
            Device device = AskForDevice();

            if (device != null)
            {
                device.FinishMaintenance();
            }
        }

        private void HandOverDevice()
        {
            Device device = AskForDevice();

            if (device != null)
            {
                device.HandOver();
            }
        }

        // تسجيل أعمال الإصلاح: قطع الغيار لكل الأجهزة، ثم أسئلة خاصة بنوع الجهاز
        private void RecordRepairWork()
        {
            Device device = AskForDevice();

            if (device == null)
            {
                return;
            }

            // نتحقق أولا حتى لا نسأل المستخدم ثم نرفض إدخاله
            if (!device.CanRecordWork)
            {
                ConsoleInput.ShowError("Repair work can only be recorded while the device is in maintenance (current stage: "
                                       + device.Stage + ").");
                return;
            }

            device.SparePartsCost = ConsoleInput.AskDecimal("Spare parts cost in $ (0-" + Device.MaxSparePartsCost + "): ",
                                                        0m, Device.MaxSparePartsCost);

            // نعرف النوع الحقيقي للجهاز بالكلمة as (تعيد null إذا لم يكن من هذا النوع)
            Fridge fridge = device as Fridge;
            Washer washer = device as Washer;
            AirCon airCon = device as AirCon;

            if (fridge != null)
            {
                fridge.CompressorReplaced = ConsoleInput.AskYesNo("Was the compressor replaced? (y/n): ");
                fridge.GasUsedKg = ConsoleInput.AskDecimal("Gas used in kg (0-" + Fridge.MaxGasKg + "): ", 0m, Fridge.MaxGasKg);
            }
            else if (washer != null)
            {
                washer.MotorReplaced = ConsoleInput.AskYesNo("Was the motor replaced? (y/n): ");
                washer.PumpReplaced = ConsoleInput.AskYesNo("Was the pump replaced? (y/n): ");
            }
            else if (airCon != null)
            {
                airCon.HomeVisit = ConsoleInput.AskYesNo("Was a home visit needed? (y/n): ");
                airCon.PipeLengthMeters = ConsoleInput.AskDecimal("Pipe length used in meters (0-" + AirCon.MaxPipeMeters + "): ",
                                                                   0m, AirCon.MaxPipeMeters);
            }

            ConsoleInput.ShowSuccess("Saved. Current repair cost: " + OutputFormat.Price(device.ComputeRepairCost()));
        }

        private void PostponeDueDate()
        {
            Device device = AskForDevice();

            if (device == null)
            {
                return;
            }

            int extraDays = ConsoleInput.AskInt("Postpone by how many days? (1-60): ", 1, 60);
            device.DueDate = device.DueDate.AddDays(extraDays);
            ConsoleInput.ShowSuccess("New due date: " + OutputFormat.Day(device.DueDate));
        }

        private void CheckOverdue()
        {
            int daysFromNow = ConsoleInput.AskInt("Check as of how many days from today? (0 = today, max 365): ", 0, 365);
            DateTime simulatedDay = DateTime.Today.AddDays(daysFromNow);

            Console.WriteLine("Checking due dates as of " + OutputFormat.Day(simulatedDay) + " ...");
            _serviceCenter.CheckAllDueDates(simulatedDay);   // هنا تطلق أحداث التأخير

            Console.WriteLine();
            PrintTable("Overdue devices", _serviceCenter.GetOverdueDevices(simulatedDay));
        }

        // ---------- التقارير ----------

        private void ShowCenterNumbers()
        {
            // هذه القيم تأتي من الخصائص الثابتة في الفئة Device مباشرة
            Console.WriteLine("--- Center numbers ---");
            Console.WriteLine("Devices registered so far      : " + Device.TotalDevices);
            Console.WriteLine("Waiting for a technician       : " + Device.WaitingCount);
            Console.WriteLine("In maintenance now             : " + Device.InMaintenanceCount);
            Console.WriteLine("Repaired so far                : " + Device.RepairedCount);
            Console.WriteLine("Handed over to customers       : " + Device.HandedOverCount);
            Console.WriteLine("Revenue from handed-over items : " + OutputFormat.Price(Device.TotalRevenue));
            Console.WriteLine("Cost of all devices in center  : " + OutputFormat.Price(_serviceCenter.ComputeTotalCost()));
        }

        private void ShowNotificationLog()
        {
            string[] notes = _serviceCenter.Desk.GetMessages();
            Console.WriteLine("--- Notification log (" + notes.Length + ") ---");

            if (notes.Length == 0)
            {
                Console.WriteLine("The log is empty.");
                return;
            }

            foreach (string note in notes)
            {
                Console.WriteLine(note);
            }
        }

        // تطبع الأجهزة في جدول. تعدد الأشكال: ComputeRepairCost و TypeName تعطي نتيجة حسب نوع كل جهاز
        private static void PrintTable(string heading, List<Device> devices)
        {
            Console.WriteLine("--- " + heading + " (" + devices.Count + ") ---");

            if (devices.Count == 0)
            {
                Console.WriteLine("Nothing to show.");
                return;
            }

            Console.WriteLine(OutputFormat.Fit("Serial", 13) + OutputFormat.Fit("Type", 17) + OutputFormat.Fit("Customer", 18)
                              + OutputFormat.Fit("Stage", 15) + OutputFormat.Fit("Due date", 12) + "Cost");
            Console.WriteLine(new string('-', 83));

            foreach (Device device in devices)
            {
                Console.WriteLine(OutputFormat.Fit(device.SerialCode, 13) + OutputFormat.Fit(device.TypeName, 17)
                                  + OutputFormat.Fit(device.CustomerName, 18) + OutputFormat.Fit(device.Stage.ToString(), 15)
                                  + OutputFormat.Fit(OutputFormat.Day(device.DueDate), 12)
                                  + OutputFormat.Price(device.ComputeRepairCost()));
            }
        }
    }
}
