using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ObjectCreationNode : ExpressionNode {
    public BasicTypeNode Type { get; }

    public ObjectCreationNode(Location location, BasicTypeNode type) :
      base(location) {
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      Type = type;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"new {Type}()";
    }
  }
}
