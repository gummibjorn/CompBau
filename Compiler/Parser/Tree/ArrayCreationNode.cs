using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ArrayCreationNode : ExpressionNode {
    public TypeNode ElementType { get; }
    public ExpressionNode Expression { get; }

    public ArrayCreationNode(Location location, TypeNode elementType, ExpressionNode expression) :
      base(location) {
      if (elementType == null) {
        throw new ArgumentNullException(nameof(elementType));
      }
      ElementType = elementType;
      if (expression == null) {
        throw new ArgumentNullException(nameof(expression));
      }
      Expression = expression;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"new {ElementType} [{Expression}]";
    }
  }
}
