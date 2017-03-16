using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class CallStatementNode : StatementNode {
    public MethodCallNode Call { get; }

    public CallStatementNode(Location location, MethodCallNode call) :
      base(location) {
      if (call == null) {
        throw new ArgumentNullException(nameof(call));
      }
      Call = call;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Call};";
    }
  }
}
