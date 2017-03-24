using System;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class ConstantSymbol : Symbol {
    public TypeSymbol Type { get; } // null for "null" constant

    public ConstantSymbol(Symbol scope, string identifier, TypeSymbol type) : 
      base(scope, identifier) {
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      Type = type;
    }
  }
}
