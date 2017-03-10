using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
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
                Token tmp;
                do
                {
                    tmp = lexer.Next();
                    Console.WriteLine(tmp.ToString());
                } while (!tmp.ToString().Equals("TOKEN End"));
                if (Diagnosis.HasErrors)
                {
                    Console.Write("Exit after lexical error(s)");
                    return;
                }
                Console.WriteLine("Success");
            }
        }
    }
}
