namespace HomeFixCenter.Contracts
{
    // الواجهة (Interface): تحدد العمليات المشتركة بين كل الأجهزة
    // دون أن تقول كيف تنفذ، فالتنفيذ مسؤولية الفئات التي تطبقها
    public interface IMaintainable
    {
        // الرقم التسلسلي للجهاز (قراءة فقط)
        string SerialCode { get; }

        // المرحلة الحالية للجهاز (قراءة فقط)
        MaintenanceStage Stage { get; }

        // بدء الصيانة
        void StartMaintenance();

        // إنهاء الصيانة بعد إصلاح الجهاز
        void FinishMaintenance();

        // تسليم الجهاز للزبون
        void HandOver();

        // حساب تكلفة الإصلاح (تختلف من جهاز لآخر)
        decimal ComputeRepairCost();

        // نص يلخص بيانات الجهاز
        string GetSummary();
    }
}
