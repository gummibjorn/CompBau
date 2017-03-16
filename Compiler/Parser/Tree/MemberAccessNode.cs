using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class MemberAccessNode : DesignatorNode {
    public DesignatorNode Designator { get; }
    public string Identifier { get; }

    public MemberAccessNode(Location location, DesignatorNode designator, string identifier) : 
      base(location) {
      if (designator == null) {
        throw new ArgumentNullException(nameof(designator));
      }
      Designator = designator;
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Designator}.{Identifier}";
    }
  }
}
