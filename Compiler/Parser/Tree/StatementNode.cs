namespace RappiSharp.Compiler.Parser.Tree {
  internal abstract class StatementNode : Node {
    public StatementNode(Location location) :
      base(location) {
    }
  }
}