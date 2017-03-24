using RappiSharp.Compiler.Parser.Tree;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class LocalVariableSymbol : VariableSymbol {
    public HashSet<StatementNode> VisibleIn { get; } = new HashSet<StatementNode>();

    public LocalVariableSymbol(Symbol scope, string identifier) : 
      base(scope, identifier) {
    }
  }
}
