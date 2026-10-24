using System;
using System.Globalization;

namespace HomeFixCenter.Tools
{
    // فئة ثابتة لقراءة ما يكتبه المستخدم؛ تعيد السؤال حتى يكتب قيمة صحيحة
    public static class ConsoleInput
    {
        // تقرأ سطرا من المستخدم بعد طباعة السؤال
        public static string AskLine(string question)
        {
            Console.Write(question);
            string typed = Console.ReadLine();

            if (typed == null)
            {
                // أغلق الإدخال (مثل Ctrl+Z) فننهي البرنامج بهدوء
                Console.WriteLine();
                Console.WriteLine("Input ended. Closing the program.");
                Environment.Exit(0);
            }

            return typed.Trim();
        }

        // تقرأ نصا طوله بين الحدين
        public static string AskText(string question, int shortest, int longest)
        {
            while (true)
            {
                string answer = AskLine(question);

                if (DataValidator.IsTextValid(answer, shortest, longest))
                {
                    return answer;
                }

                ShowError("Please type between " + shortest + " and " + longest + " characters.");
            }
        }

        // تقرأ اسم شخص صحيحا
        public static string AskPersonName(string question)
        {
            while (true)
            {
                string answer = AskLine(question);

                if (DataValidator.IsPersonNameValid(answer))
                {
                    return answer;
                }

                ShowError("Use 2-40 characters: letters, spaces, dots or dashes only.");
            }
        }

        // تقرأ عددا صحيحا ضمن مجال محدد
        public static int AskInt(string question, int lowest, int highest)
        {
            while (true)
            {
                string answer = AskLine(question);
                int value;

                if (int.TryParse(answer, out value) && value >= lowest && value <= highest)
                {
                    return value;
                }

                ShowError("Please type a whole number from " + lowest + " to " + highest + ".");
            }
        }

        // تقرأ عددا عشريا ضمن مجال محدد (نستعمل النقطة للفاصلة العشرية مثل 2.5)
        public static decimal AskDecimal(string question, decimal lowest, decimal highest)
        {
            while (true)
            {
                string answer = AskLine(question);
                decimal value;

                if (decimal.TryParse(answer, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value)
                    && value >= lowest && value <= highest)
                {
                    return value;
                }

                ShowError("Please type a number from " + lowest + " to " + highest + " (use a dot, like 2.5).");
            }
        }

        // تقرأ جوابا بنعم أو لا (y أو n)
        public static bool AskYesNo(string question)
        {
            while (true)
            {
                string answer = AskLine(question).ToLower();

                if (answer == "y" || answer == "yes")
                {
                    return true;
                }

                if (answer == "n" || answer == "no")
                {
                    return false;
                }

                ShowError("Type y for yes or n for no.");
            }
        }

        // رسالة خطأ باللون الأحمر
        public static void ShowError(string note)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("   Error: " + note);
            Console.ResetColor();
        }

        // رسالة نجاح باللون الأخضر
        public static void ShowSuccess(string note)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("   " + note);
            Console.ResetColor();
        }
    }
}
