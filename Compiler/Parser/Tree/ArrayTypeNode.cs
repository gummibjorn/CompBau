using System;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ArrayTypeNode : TypeNode {
    public TypeNode ElementType { get; }

    public ArrayTypeNode(Location location, TypeNode elementType) :
      base(location) {
      if (elementType == null) {
        throw new ArgumentNullException(nameof(elementType));
      }
      ElementType = elementType;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      return $"{ElementType}[]";
    }
  }
}
