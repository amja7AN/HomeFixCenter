using HomeFixCenter.Contracts;
using HomeFixCenter.Devices;

namespace HomeFixCenter.Alerts
{
    // التفويض (Delegate) الأول: يحدد شكل أي دالة تريد أن تعرف بتغير مرحلة الجهاز
    public delegate void StageChangedHandler(Device device, MaintenanceStage oldStage, MaintenanceStage newStage);

    // التفويض الثاني: يحدد شكل أي دالة تريد أن تعرف بتأخر تسليم جهاز
    public delegate void OverdueHandler(Device device, int lateDays);
}
