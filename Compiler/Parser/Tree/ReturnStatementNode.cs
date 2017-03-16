namespace RappiSharp.Compiler.Parser.Tree {
  internal class ReturnStatementNode : StatementNode {
    public ExpressionNode Expression { get; } // null if none
    
    public ReturnStatementNode(Location location, ExpressionNode expression) :
      base(location) {
      Expression = expression;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = "return";
      if (Expression != null) {
        text += Expression;
      }
      text += ";";
      return text;
    }
  }
}
