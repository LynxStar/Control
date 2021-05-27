using System;

namespace Shift.Language
{
    public static class Test
    {
        public static void Check<T>(T actual, T expected)
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
            Console.WriteLine(output);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void True(bool actual)
        {
            Check(actual, true);
        }
    }
}
