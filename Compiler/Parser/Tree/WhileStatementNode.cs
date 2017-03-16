using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class WhileStatementNode : StatementNode {
    public ExpressionNode Condition { get; }
    public StatementBlockNode Body { get; }

    public WhileStatementNode(Location location, ExpressionNode condition, StatementBlockNode body) :
      base(location) {
      if (condition == null) {
        throw new ArgumentNullException(nameof(condition));
      }
      Condition = condition;
      if (body == null) {
        throw new ArgumentNullException(nameof(body));
      }
      Body = body;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"while ({Condition}) {{\r\n{Body}\r\n}}";
    }
  }
}
