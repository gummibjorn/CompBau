using System;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal abstract class TypeSymbol : Symbol {
    public TypeSymbol(Symbol scope, string identifier) : 
      base(scope, identifier) {
      if (scope == null) {
        throw new ArgumentNullException(nameof(scope));
      }
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
    }
  }
}
