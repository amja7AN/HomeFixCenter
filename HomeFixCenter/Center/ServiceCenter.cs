using System;
using System.Collections.Generic;
using HomeFixCenter.Alerts;
using HomeFixCenter.Contracts;
using HomeFixCenter.Devices;
using HomeFixCenter.Tools;

namespace HomeFixCenter.Center
{
    // الفئة الرئيسية: تحتفظ بقائمة من نوع Device (الفئة المجردة) فيها ثلاجات وغسالات ومكيفات معا.
    // عند المرور على القائمة نستدعي ComputeRepairCost دون أن نعرف نوع كل جهاز،
    // وكل جهاز ينفذ الدالة بطريقته (تعدد الأشكال)
    public class ServiceCenter
    {
        private readonly string _centerName;
        private readonly List<Device> _devices = new List<Device>();
        private readonly NotificationDesk _desk = new NotificationDesk();

        public ServiceCenter(string centerName)
        {
            if (!DataValidator.IsTextValid(centerName, 2, 60))
            {
                throw new ArgumentException("Center name must be between 2 and 60 characters.");
            }

            _centerName = centerName.Trim();
        }

        public string CenterName
        {
            get { return _centerName; }
        }

        public NotificationDesk Desk
        {
            get { return _desk; }
        }

        // يضيف جهازا جديدا ويربطه بمكتب الإشعارات ليستقبل أحداثه
        public void AddDevice(Device device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (GetBySerial(device.SerialCode) != null)
            {
                throw new InvalidOperationException("A device with serial number " + device.SerialCode + " is already registered.");
            }

            _devices.Add(device);
            _desk.Watch(device);
        }

        // يبحث عن جهاز بالرقم التسلسلي ويعيد null إذا لم يجده
        public Device GetBySerial(string serialCode)
        {
            if (string.IsNullOrWhiteSpace(serialCode))
            {
                return null;
            }

            string lookupKey = serialCode.Trim().ToUpper();

            foreach (Device device in _devices)
            {
                if (device.SerialCode == lookupKey)
                {
                    return device;
                }
            }

            return null;
        }

        // كل الأجهزة (نسخة من القائمة حتى لا يعدلها أحد من الخارج)
        public List<Device> GetAllDevices()
        {
            return new List<Device>(_devices);
        }

        // الأجهزة الموجودة في مرحلة معينة
        public List<Device> GetByStage(MaintenanceStage stage)
        {
            List<Device> result = new List<Device>();

            foreach (Device device in _devices)
            {
                if (device.Stage == stage)
                {
                    result.Add(device);
                }
            }

            return result;
        }

        // الأجهزة التي تم إصلاحها (جاهزة للاستلام أو مسلمة)
        public List<Device> GetRepairedDevices()
        {
            List<Device> result = new List<Device>();

            foreach (Device device in _devices)
            {
                if (device.Stage == MaintenanceStage.Repaired || device.Stage == MaintenanceStage.HandedOver)
                {
                    result.Add(device);
                }
            }

            return result;
        }

        // الأجهزة المتأخرة عن موعد التسليم في التاريخ المعطى
        public List<Device> GetOverdueDevices(DateTime asOf)
        {
            List<Device> result = new List<Device>();

            foreach (Device device in _devices)
            {
                if (device.IsOverdue(asOf))
                {
                    result.Add(device);
                }
            }

            return result;
        }

        // يفحص كل الأجهزة، فتطلق المتأخرة منها الحدث DeliveryOverdue
        public void CheckAllDueDates(DateTime asOf)
        {
            foreach (Device device in _devices)
            {
                device.CheckDueDate(asOf);
            }
        }

        // مجموع تكلفة كل الأجهزة. السطر داخل الحلقة يستدعي دالة مختلفة حسب النوع الحقيقي للجهاز
        public decimal ComputeTotalCost()
        {
            decimal sum = 0m;

            foreach (Device device in _devices)
            {
                sum += device.ComputeRepairCost();
            }

            return sum;
        }
    }
}
