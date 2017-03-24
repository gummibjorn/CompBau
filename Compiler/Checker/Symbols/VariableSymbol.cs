using System;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal abstract class VariableSymbol : Symbol {
    public TypeSymbol Type { get; set; }

    public VariableSymbol(Symbol scope, string identifier) :
      base(scope, identifier) {
      if (scope == null) {
        throw new ArgumentNullException(nameof(scope));
      }
    }
  }
}
