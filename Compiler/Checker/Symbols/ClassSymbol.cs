using System.Collections.Generic;
using System.Linq;

namespace RappiSharp.Compiler.Checker.Symbols {
  internal class ClassSymbol : TypeSymbol {
    public const string This = "this";

    public TypeSymbol BaseClass { get; set; }
    public List<FieldSymbol> Fields { get; } = new List<FieldSymbol>();
    public List<MethodSymbol> Methods { get; } = new List<MethodSymbol>();

    private readonly FieldSymbol _this;
    
    public ClassSymbol(Symbol scope, string identifier) :
      base(scope, identifier) {
      _this = new FieldSymbol(this, This) { Type = this };
    }

    public override IEnumerable<Symbol> AllDeclarations {
      get {
        var declarations = Fields.Cast<Symbol>().Union(Methods).ToList();
        declarations.Add(_this);
        if (BaseClass != null) {
          foreach (var baseDeclaration in BaseClass.AllDeclarations) {
            if (!declarations.Any(symbol => symbol.Identifier == baseDeclaration.Identifier)) {
              declarations.Add(baseDeclaration);
            }
          }
        }
        return declarations;
      }
    }

    public IEnumerable<ClassSymbol> TransitiveBaseClasses {
      get {
        var result = new List<ClassSymbol>();
        if (BaseClass is ClassSymbol) {
          var direct = (ClassSymbol)BaseClass;
          result.Add(direct);
          foreach (var indirect in direct.TransitiveBaseClasses) {
            result.Add(indirect);
          }
        }
        return result;
      }
    }

    public List<FieldSymbol> AllFields {
      get {
        List<FieldSymbol> result;
        if (BaseClass is ClassSymbol) {
          result = ((ClassSymbol)BaseClass).AllFields;
        } else {
          result = new List<FieldSymbol>();
        }
        foreach (var field in Fields) {
          result.Add(field);
        }
        return result;
      }
    }
  }
}
