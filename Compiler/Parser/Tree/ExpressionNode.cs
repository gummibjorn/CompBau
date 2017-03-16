namespace RappiSharp.Compiler.Parser.Tree {
  internal abstract class ExpressionNode : Node {
    public ExpressionNode(Location location) : 
      base(location) {
    }
  }
}
