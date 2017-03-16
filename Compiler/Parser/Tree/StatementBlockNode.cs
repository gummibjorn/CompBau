using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class StatementBlockNode : Node {
    public List<StatementNode> Statements { get; }

    public StatementBlockNode(Location location, List<StatementNode> statements) :
      base(location) {
      if (statements == null) {
        throw new ArgumentNullException(nameof(statements));
      }
      Statements = statements;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = "{\r\n";
      foreach (var statement in Statements) {
        text += statement + "\r\n";
      }
      text += "}";
      return text;
    }
  }
}
