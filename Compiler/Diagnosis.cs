﻿using System;
using System.IO;

namespace RappiSharp.Compiler
{
    internal static class Diagnosis
    {
        public static bool HasErrors { get; private set; } = false;

        public static string Messages = "";

        public static string source;

        public static void ReportError(Location location, string errorMessage)
        {
            var message = $"ERROR {errorMessage} at {location}\n" + printCodeLine(location);
            Console.WriteLine(message);
            Messages += message + "\r\n";
            HasErrors = true;
        }

        private static string printCodeLine(Location location)
        {
            if(source == null) { return ""; }
            if(location.Row < 0) { return "CANNOT PRINT CODE FOR ROW " + location.Row; }
            string result = "";
            var lines = source.Split('\n');
            result += lines[location.Row - 1];
            result += "\r\n";
            for(var i=0; i< location.Col-1; i++)
            {
                result += ' ';
            }
            result += "^\n";
            return result;
        }


        public static void Clear()
        {
            HasErrors = false;
            Messages = "";
        }
    }
}
