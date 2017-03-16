using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class UnaryExpressionNode : ExpressionNode {
    public Operator Operator { get; }
    public ExpressionNode Operand { get; }
    
    public UnaryExpressionNode(Location location, Operator op, ExpressionNode operand) :
      base(location) {
      Operator = op;
      if (operand == null) {
        throw new ArgumentNullException(nameof(operand));
      }
      Operand = operand;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"({Operator} {Operand})";
    }
  }
}
