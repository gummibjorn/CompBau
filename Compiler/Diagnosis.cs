using System;

namespace RappiSharp.Compiler
{
    internal static class Diagnosis
    {
        public static bool HasErrors { get; private set; } = false;

        public static string Messages = "";

        public static void ReportError(Location location, string errorMessage)
        {

            var message = $"ERROR {errorMessage} at {location}";
            Console.WriteLine(message);
            Messages += message + "\r\n";
            HasErrors = true;
        }

        public static void Clear()
        {
            HasErrors = false;
            Messages = "";
        }
    }
}
