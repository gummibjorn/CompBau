namespace RappiSharp.Compiler.Lexer.Tokens {
  internal class IntegerToken : Token {
    public uint Value { get; }

    public IntegerToken(Location location, uint value) : 
      base(location) {
      Value = value;
    }

    public override string ToString() {
      return $"INTEGER {Value}";
    }
  }
}
