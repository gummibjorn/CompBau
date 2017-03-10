using System;

namespace RappiSharp.Compiler {
  internal static class Diagnosis {
    public static bool HasErrors { get; private set; } = false;

    public static void ReportError(Location location, string message) {
      Console.WriteLine($"ERROR {message} at {location}");
      HasErrors = true;
    }

    public static void Clear() {
      HasErrors = false;
    }
  }
}
