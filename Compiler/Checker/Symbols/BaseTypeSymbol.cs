using System;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class BaseTypeSymbol : TypeSymbol {
    public BaseTypeSymbol(Symbol scope, string identifier) : 
      base(scope, identifier) {
      if (scope == null) {
        throw new ArgumentNullException(nameof(scope));
      }
    }
  }
}
