namespace RappiSharp.Compiler.Lexer.Tokens {
  internal class IntegerToken : Token {
    public int Value { get; }

    public IntegerToken(Location? location, int value) : 
      base(location) {
      Value = value;
    }

    public override string ToString() {
      return $"INTEGER {Value}";
    }
  }
}
