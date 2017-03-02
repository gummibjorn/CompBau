using System;

namespace RappiSharp.Compiler {
  internal static class Diagnosis {
    public static bool HasErrors { get; private set; } = false;

    public static void ReportError(string message) {
      Console.WriteLine($"ERROR {message}");
      HasErrors = true;
    }

    public static void Clear() {
      HasErrors = false;
    }
  }
}
