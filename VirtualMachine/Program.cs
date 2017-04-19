using RappiSharp.IL;
using RappiSharp.VirtualMachine.Error;
using RappiSharp.VirtualMachine.Runtime;
using System;

namespace RappiSharp.VirtualMachine {
  public class Program {
    public static void Main(string[] arguments) {
      if (arguments.Length != 1) {
        Console.Write("Usage: rvm <file>\r\n");
        return;
      }
      var file = arguments[0];
      Metadata metadata = Metadata.Load(file);
      try {
        var interpreter = new Interpreter(metadata);
        interpreter.Run();
      } catch (VMException exception) {
        Console.Write($"VM ERROR: {exception.Message}\r\n");
      }
    }
  }
}
