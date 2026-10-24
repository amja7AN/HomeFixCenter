using System;

namespace HomeFixCenter.Tools
{
    // فئة ثابتة (static class): تجمع دوال التحقق من صحة البيانات.
    // لا ننشئ منها كائنات، بل نستدعي دوالها مباشرة مثل: DataValidator.IsSerialValid("RF-2026-014")
    public static class DataValidator
    {
        // الرقم التسلسلي بالشكل: حرفان - أربعة أرقام - ثلاثة أرقام، مثال RF-2026-014
        public static bool IsSerialValid(string serialCode)
        {
            if (string.IsNullOrWhiteSpace(serialCode))
            {
                return false;
            }

            string content = serialCode.Trim();

            if (content.Length != 11)
            {
                return false;
            }

            for (int i = 0; i < content.Length; i++)
            {
                char c = content[i];

                if (i < 2)
                {
                    // أول حرفين يجب أن يكونا حروفا إنجليزية
                    if (!IsEnglishLetter(c))
                    {
                        return false;
                    }
                }
                else if (i == 2 || i == 7)
                {
                    // مكان الشرطتين
                    if (c != '-')
                    {
                        return false;
                    }
                }
                else if (c < '0' || c > '9')
                {
                    // باقي الخانات أرقام
                    return false;
                }
            }

            return true;
        }

        // اسم الشخص: من 2 إلى 40 حرفا، يبدأ بحرف، ويحتوي حروفا ومسافات ونقطة وشرطة فقط
        public static bool IsPersonNameValid(string personName)
        {
            if (string.IsNullOrWhiteSpace(personName))
            {
                return false;
            }

            string content = personName.Trim();

            if (content.Length < 2 || content.Length > 40 || !char.IsLetter(content[0]))
            {
                return false;
            }

            foreach (char c in content)
            {
                bool allowed = char.IsLetter(c) || c == ' ' || c == '.' || c == '-';

                if (!allowed)
                {
                    return false;
                }
            }

            return true;
        }

        // نص عادي غير فارغ وطوله بين الحدين
        public static bool IsTextValid(string content, int shortest, int longest)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            int size = content.Trim().Length;
            return size >= shortest && size <= longest;
        }

        // تاريخ الاستلام: ليس في المستقبل وليس أقدم من 5 سنوات
        public static bool IsReceivedDateValid(DateTime day)
        {
            DateTime asOf = DateTime.Today;
            return day.Date <= asOf && day.Date >= asOf.AddYears(-5);
        }

        // موعد التسليم لا يسبق تاريخ الاستلام
        public static bool IsDueDateValid(DateTime receivedOn, DateTime dueDate)
        {
            return dueDate.Date >= receivedOn.Date;
        }

        // هل القيمة ضمن المجال من min إلى max؟
        public static bool IsBetween(decimal value, decimal lowest, decimal highest)
        {
            return value >= lowest && value <= highest;
        }

        // دالة مساعدة خاصة بهذه الفئة
        private static bool IsEnglishLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }
    }
}
