using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class TypeCastNode : ExpressionNode {
    public BasicTypeNode Type { get; } 
    public DesignatorNode Designator { get; }
    
    public TypeCastNode(Location location, BasicTypeNode type, DesignatorNode designator) :
      base(location) {
      if (type == null) {
        throw new ArgumentNullException(nameof(type));
      }
      Type = type;
      if (designator == null) {
        throw new ArgumentNullException(nameof(designator));
      }
      Designator = designator;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"({Type}){Designator}";
    }
  }
}
