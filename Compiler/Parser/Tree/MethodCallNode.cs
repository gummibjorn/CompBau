using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class MethodCallNode : ExpressionNode {
    public DesignatorNode Designator { get; }
    public List<ExpressionNode> Arguments { get; }
    
    public MethodCallNode(Location location, DesignatorNode designator, List<ExpressionNode> arguments) :
      base(location) {
      if (designator == null) {
        throw new ArgumentNullException(nameof(designator));
      }
      Designator = designator;
      if (arguments == null) {
        throw new ArgumentNullException(nameof(arguments));
      }
      Arguments = arguments;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = $"{Designator}(";
      for (int index = 0; index < Arguments.Count; index++) {
        text += Arguments[index];
        if (index < Arguments.Count - 1) {
          text += ", ";
        }
      }
      text += ")";
      return text;
    }
  }
}