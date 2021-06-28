using System;

namespace Shift.Language
{
    public static class Test
    {
        public static void Check<T>(T actual, T expected, string name)
        {
            var result = System.Collections.Generic.EqualityComparer<T>.Default.Equals(actual, expected);

            var color = result
                ? ConsoleColor.Green
                : ConsoleColor.Red
                ;

            string output = result
                ? "Pass"
                : "Fail"
                ;

            Console.ForegroundColor = color;
            Console.Write(output);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" - {name}");
        }

        public static void True(bool actual, string name)
        {
            Check(actual, true, name);
        }

        public static void False(bool actual, string name)
        {
            Check(actual, false, name);
        }

    }
}
