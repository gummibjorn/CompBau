using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class AssignmentNode : StatementNode {
    public DesignatorNode Left { get; }
    public ExpressionNode Right { get; }

    public AssignmentNode(Location location, DesignatorNode left, ExpressionNode right) :
      base(location) {
      if (left == null) {
        throw new ArgumentNullException(nameof(left));
      }
      Left = left;
      if (right == null) {
        throw new ArgumentNullException(nameof(right));
      }
      Right = right;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Left} = {Right};";
    }
  }
}
