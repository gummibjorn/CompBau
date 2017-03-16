using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class IfStatementNode : StatementNode {
    public ExpressionNode Condition { get; }
    public StatementBlockNode Then { get; }
    public StatementBlockNode Else { get; } // null if without else
    
    public IfStatementNode(Location location, ExpressionNode condition, StatementBlockNode then, StatementBlockNode elseBlock) :
      base(location) {
      if (condition == null) {
        throw new ArgumentNullException(nameof(condition));
      }
      Condition = condition;
      if (then == null) {
        throw new ArgumentNullException(nameof(then));
      }
      Then = then;
      Else = elseBlock;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = $"if ({Condition}) {{\r\n{Then}\r\n}}";
      if (Else != null) {
        text += $"else {{\r\n{Else}\r\n}}";
      }
      return text;
    }
  }
}
