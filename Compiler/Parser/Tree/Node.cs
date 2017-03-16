namespace RappiSharp.Compiler.Parser.Tree {
  internal abstract class Node {
    public Location Location { get; }

    public Node(Location location) {
      Location = location;
    }

    public abstract void Accept(Visitor visitor);

    public abstract override string ToString();
  }
}