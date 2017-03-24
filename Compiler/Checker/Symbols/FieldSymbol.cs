namespace RappiSharp.Compiler.Checker.Symbols {
  internal class FieldSymbol : VariableSymbol {
    public FieldSymbol(Symbol scope, string identifier) : 
      base(scope, identifier) {
    }
  }
}
