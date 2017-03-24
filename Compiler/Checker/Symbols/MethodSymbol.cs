using System;
using System.Collections.Generic;
using System.Linq;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class MethodSymbol : Symbol {
    public TypeSymbol ReturnType { get; set; } // null if void
    public List<ParameterSymbol> Parameters { get; } = new List<ParameterSymbol>();
    public List<LocalVariableSymbol> LocalVariables { get; } = new List<LocalVariableSymbol>();

    public MethodSymbol(Symbol scope, string identifier) :
      base(scope, identifier) {
      if (scope == null) {
        throw new ArgumentNullException(nameof(scope));
      }
    }

    public override IEnumerable<Symbol> AllDeclarations {
      get {
        return Parameters.Cast<Symbol>().Union(LocalVariables);
      }
    }
  }
}