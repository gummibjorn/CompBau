using System.Collections.Generic;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal abstract class Symbol {
    public Symbol Scope { get; }
    public string Identifier { get; }
    
    public Symbol(Symbol scope, string identifier) {
      Scope = scope;
      Identifier = identifier;
    }

    public virtual IEnumerable<Symbol> AllDeclarations {
      get {
        return new List<Symbol>();
      }
    }

    public override string ToString() {
      return Identifier;
    }
  }
}
