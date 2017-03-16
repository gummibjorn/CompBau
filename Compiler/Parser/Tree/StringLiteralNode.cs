using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class StringLiteralNode : ExpressionNode {
    public string Value { get; }

    public StringLiteralNode(Location location, string value) : 
      base(location) {
      if (value == null) {
        throw new ArgumentNullException(nameof(value));
      }
      Value = value;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"\"{Value}\"";
    }
  }
}
