using System;

namespace HomeFixCenter.Tools
{
    // فئة ثابتة لتنسيق القيم عند عرضها، حتى يكون الشكل واحدا في كل البرنامج
    public static class OutputFormat
    {
        // مثال: $125.40
        public static string Price(decimal dollars)
        {
            return "$" + dollars.ToString("0.00");
        }

        // مثال: 2026-11-12
        public static string Day(DateTime day)
        {
            return day.ToString("yyyy-MM-dd");
        }

        // يحول القيمة المنطقية إلى Yes أو No
        public static string ToYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }

        // يقص النص إذا كان أطول من العرض المطلوب ثم يكمله بمسافات، لتبقى أعمدة الجدول مرتبة
        public static string Fit(string content, int width)
        {
            if (content.Length > width - 1)
            {
                content = content.Substring(0, width - 3) + "..";
            }

            return content.PadRight(width);
        }
    }
}
