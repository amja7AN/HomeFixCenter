namespace HomeFixCenter.Contracts
{
    // مراحل رحلة الجهاز داخل المركز، مرتبة من الاستلام حتى التسليم
    public enum MaintenanceStage
    {
        Registered,     // تم تسجيل الجهاز وهو بانتظار الفني
        InMaintenance,  // الجهاز قيد الصيانة الآن
        Repaired,       // انتهى الإصلاح والجهاز جاهز للاستلام
        HandedOver      // تم تسليم الجهاز للزبون
    }
}
