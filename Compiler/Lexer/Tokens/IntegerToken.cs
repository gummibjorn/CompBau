namespace RappiSharp.Compiler.Lexer.Tokens {
  internal class IntegerToken : Token {
    public long Value { get; }

    public IntegerToken(Location? location, long value) : 
      base(location) {
      Value = value;
    }

    public override string ToString() {
      return $"INTEGER {Value}";
    }
  }
}
