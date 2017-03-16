using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class VariableNode : Node {
    public TypeNode Type { get; }
    public string Identifier { get; }

    public VariableNode(Location node, TypeNode type, string identifier) :
      base(node) {
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      Type = type;
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Type} {Identifier};";
    }
  }
}
