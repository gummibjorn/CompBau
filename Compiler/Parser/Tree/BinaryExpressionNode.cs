using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class BinaryExpressionNode : ExpressionNode {
    public ExpressionNode Left { get; }
    public Operator? Operator { get; }
    public ExpressionNode Right { get; }
    
    public BinaryExpressionNode(Location location, ExpressionNode left, Operator? op, ExpressionNode right) :
      base(location) {
      if (left == null) {
        throw new ArgumentNullException(nameof(left));
      }
      Left = left;
      Operator = op;
      if (right == null) {
        throw new ArgumentNullException(nameof(right));
      }
      Right = right;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"({Left} {Operator} {Right})";
    }
  }
}
