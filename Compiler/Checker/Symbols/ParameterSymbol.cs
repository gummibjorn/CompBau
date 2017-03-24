namespace RappiSharp.Compiler.Checker.Symbols {
  internal class ParameterSymbol : VariableSymbol {
    public ParameterSymbol(Symbol scope, string identifier) : 
      base(scope, identifier) {
    }
  }
}
