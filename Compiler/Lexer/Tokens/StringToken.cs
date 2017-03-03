namespace RappiSharp.Compiler.Lexer.Tokens {
  internal class StringToken : Token {
    public string Value { get; }

    public StringToken(Location? location, string value) :
      base(location) {
      Value = value;
    }

    public override string ToString() {
      return $"STRING \"{Value}\"";
    }
  }
}
