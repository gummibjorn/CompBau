using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler.Generator;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using System;
using System.IO;
using System.Text;

namespace RappiSharp.Compiler {
  public class Program {
    public static void Main(string[] arguments) {
      if (arguments.Length != 1) {
        Console.Write("Usage: rsc <file>\r\n");
        return;
      }
      var inputFile = arguments[0];
      var outputFile = ReplaceExtension(inputFile, ".rs", ".il");
      using (var reader = new StreamReader(File.OpenRead(inputFile), Encoding.ASCII)) {
        Diagnosis.Clear();
        var lexer = new RappiLexer(reader);
        if (Diagnosis.HasErrors) {
          Console.Write("Exit after lexical error(s)\r\n");
          return;
        }
        var parser = new RappiParser(lexer);
        var tree = parser.ParseProgram();
        if (Diagnosis.HasErrors) {
          Console.Write("Exit after syntactic error(s)\r\n");
          return;
        }
        var checker = new RappiChecker(tree);
        var table = checker.SymbolTable;
        if (Diagnosis.HasErrors) {
          Console.Write("Exit after semantic error(s)\r\n");
          return;
        }
        var generator = new RappiGenerator(table);
        var metadata = generator.Metadata;
        if (Diagnosis.HasErrors) {
          Console.Write("Exit after generation error(s)\r\n");
          return;
        }
        metadata.Save(outputFile);
        Console.Write("Success\r\n");
      }
    }

    private static string ReplaceExtension(string filePath, string oldExtension, string newExtension) {
      if (!filePath.EndsWith(oldExtension)) {
        return filePath + newExtension;
      } else {
        return filePath.Remove(filePath.Length - oldExtension.Length) + newExtension;
      }
    }
  }
}
