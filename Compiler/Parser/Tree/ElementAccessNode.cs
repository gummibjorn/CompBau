using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ElementAccessNode : DesignatorNode {
    public DesignatorNode Designator { get; }
    public ExpressionNode Expression { get; }

    public ElementAccessNode(Location location, DesignatorNode designator, ExpressionNode expression) : 
      base(location) {
      if (designator == null) {
        throw new ArgumentNullException(nameof(designator));
      }
      Designator = designator;
      Expression = expression;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{Designator}[{Expression}]";
    }
  }
}
