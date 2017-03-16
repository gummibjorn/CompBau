using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class LocalDeclarationNode : StatementNode {
    public VariableNode Variable { get; }

    public LocalDeclarationNode(Location location, VariableNode variable) : 
      base(location) {
      if (variable == null) {
        throw new ArgumentNullException(nameof(variable));
      }
      Variable = variable;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return Variable.ToString();
    }
  }
}
