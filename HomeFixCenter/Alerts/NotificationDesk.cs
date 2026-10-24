using System;
using System.Collections.Generic;
using HomeFixCenter.Contracts;
using HomeFixCenter.Devices;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Alerts
{
    // مكتب الإشعارات: يستقبل أحداث الأجهزة ويكتب رسالة للزبون ويحفظها في سجل.
    // الدالتان HandleStageChanged و HandleOverdue لهما نفس توقيع التفويضين
    // لذلك يمكن ربطهما بالحدثين StageChanged و DeliveryOverdue
    public class NotificationDesk
    {
        // سجل الرسائل (خاص، ويقرأ فقط عن طريق GetMessages)
        private readonly List<string> _messages = new List<string>();

        // نشترك في أحداث الجهاز: عند وقوع الحدث تنفذ الدالة المربوطة تلقائيا
        public void Watch(Device device)
        {
            device.StageChanged += HandleStageChanged;
            device.DeliveryOverdue += HandleOverdue;
        }

        // تنفذ كلما تغيرت مرحلة جهاز
        private void HandleStageChanged(Device device, MaintenanceStage oldStage, MaintenanceStage newStage)
        {
            string content;
            ConsoleColor textColor = ConsoleColor.Cyan;

            switch (newStage)
            {
                case MaintenanceStage.InMaintenance:
                    content = "Our technician has started working on your " + device.TypeName + ".";
                    break;
                case MaintenanceStage.Repaired:
                    // الشرط الأهم: اكتمال الإصلاح
                    content = "Good news! Your " + device.TypeName + " is fixed and ready for pick-up. Amount due: "
                           + OutputFormat.Price(device.ComputeRepairCost()) + ".";
                    textColor = ConsoleColor.Green;
                    break;
                case MaintenanceStage.HandedOver:
                    content = "Your " + device.TypeName + " was handed over. Thank you for choosing us!";
                    break;
                default:
                    content = "Stage changed from " + oldStage + " to " + newStage + ".";
                    break;
            }

            Publish("UPDATE", device, content, textColor);
        }

        // تنفذ عند اكتشاف تأخر في تسليم جهاز
        private void HandleOverdue(Device device, int lateDays)
        {
            string content = "Sorry, your " + device.TypeName + " is " + lateDays
                          + " day(s) late. It was due on " + OutputFormat.Day(device.DueDate) + ".";
            Publish("LATE", device, content, ConsoleColor.Yellow);
        }

        // تكتب الرسالة على الشاشة وتحفظها في السجل
        private void Publish(string label, Device device, string content, ConsoleColor textColor)
        {
            string row = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + label + " to "
                          + device.CustomerName + " (" + device.SerialCode + "): " + content;
            _messages.Add(row);

            Console.ForegroundColor = textColor;
            Console.WriteLine("   >> " + row);
            Console.ResetColor();
        }

        // تعيد نسخة من السجل حتى لا يعدل أحد القائمة الأصلية
        public string[] GetMessages()
        {
            return _messages.ToArray();
        }
    }
}
