using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using System;
using System.IO;

namespace RappiSharp.Compiler
{
    public class Program
    {
        public static void Main(string[] arguments)
        {
            if (arguments.Length != 1)
            {
                Console.WriteLine("Usage: rsc <file>");
                return;
            }
            var inputFile = arguments[0];
            using (var reader = File.OpenText(inputFile))
            {
                Diagnosis.Clear();
                var lexer = new RappiLexer(reader);
                if (Diagnosis.HasErrors)
                {
                    Console.Write("Exit after lexical error(s)");
                    return;
                }
                var parser = new RappiParser(lexer);
                //var tree = parser.Parse();
                parser.ParseProgram();
                if (Diagnosis.HasErrors)
                {
                    Console.Write("Exit after syntactic error(s)");
                    return;
                }
                Console.WriteLine("Success");
            }
        }
    }
}
