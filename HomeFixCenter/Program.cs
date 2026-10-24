using System;
using System.Globalization;
using System.Threading;
using HomeFixCenter.Center;
using HomeFixCenter.Screens;
using HomeFixCenter.Tools;

namespace HomeFixCenter
{
    // نقطة بداية البرنامج: نظام إدارة مركز صيانة الأجهزة المنزلية
    internal class Program
    {
        private static void Main()
        {
            // نثبت تنسيق الأرقام والتواريخ حتى لا يتغير شكلها بتغير إعدادات الجهاز
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            ServiceCenter serviceCenter = new ServiceCenter("HomeFix Appliance Service Center");

            Console.WriteLine("Welcome to " + serviceCenter.CenterName);
            Console.WriteLine();

            bool loadSamples = ConsoleInput.AskYesNo("Load sample devices for a quick demo? (y/n): ");

            if (loadSamples)
            {
                int loadedCount = SampleDevices.Fill(serviceCenter);
                ConsoleInput.ShowSuccess(loadedCount + " sample devices were loaded.");
            }

            Console.WriteLine();

            MainMenu mainMenu = new MainMenu(serviceCenter);
            mainMenu.Start();
        }
    }
}
