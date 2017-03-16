namespace RappiSharp.Compiler.Parser.Tree {
  internal class IntegerLiteralNode : ExpressionNode {
    public int Value { get; }

    public IntegerLiteralNode(Location location, int value) :
      base(location) {
      Value = value;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return Value.ToString();
    }
  }
}
