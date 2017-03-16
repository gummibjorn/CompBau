using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class BasicDesignatorNode : DesignatorNode {
    public string Identifier { get; }

    public BasicDesignatorNode(Location location, string identifier) : 
      base(location) {
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return Identifier;
    }
  }
}
