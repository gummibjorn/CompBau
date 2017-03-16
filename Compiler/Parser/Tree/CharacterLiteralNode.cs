namespace RappiSharp.Compiler.Parser.Tree {
  internal class CharacterLiteralNode : ExpressionNode {
    public char Value { get; }
    
    public CharacterLiteralNode(Location location, char value) : 
      base(location) {
      Value = value;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"'{Value}'";
    }
  }
}
