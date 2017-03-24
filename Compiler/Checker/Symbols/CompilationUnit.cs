using System.Collections.Generic;
using System.Linq;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class CompilationUnit : Symbol {
    public List<ClassSymbol> Classes { get; } = new List<ClassSymbol>();
    public List<TypeSymbol> Types { get; } = new List<TypeSymbol>(); 
    public List<MethodSymbol> Methods { get; } = new List<MethodSymbol>(); // predefined methods
    public List<ConstantSymbol> Constants { get; } = new List<ConstantSymbol>(); // predefined constants

    public TypeSymbol NullType { get; }
    public TypeSymbol BoolType { get; set; }
    public TypeSymbol CharType { get; set; }
    public TypeSymbol IntType { get; set; }
    public TypeSymbol StringType { get; set; }

    public ConstantSymbol TrueConstant { get; set; }
    public ConstantSymbol FalseConstant { get; set; }
    public ConstantSymbol NullConstant { get; set; }

    public FieldSymbol ArrayLength { get; set; }

    public MethodSymbol MainMethod { get; set; }

    public CompilationUnit() : 
      base(null, null) {
      NullType = new BaseTypeSymbol(this, "@NULL");
    }

    public override IEnumerable<Symbol> AllDeclarations {
      get {
        return Types.Cast<Symbol>().Union(Methods).Union(Constants);
      }
    }
  }
}
