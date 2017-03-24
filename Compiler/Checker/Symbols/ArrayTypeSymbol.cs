using System;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class ArrayTypeSymbol : TypeSymbol {
    public TypeSymbol ElementType { get; }

    public ArrayTypeSymbol(Symbol scope, TypeSymbol elementType) : 
      base(scope, elementType.Identifier + "[]") {
      if (scope == null) {
        throw new ArgumentNullException(nameof(scope));
      }
      if (elementType == null) {
        throw new ArgumentNullException(nameof(elementType));
      }
      ElementType = elementType;
    }
  }
}
