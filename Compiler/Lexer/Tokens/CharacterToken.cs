namespace RappiSharp.Compiler.Lexer.Tokens {
  internal class CharacterToken : Token {
    public char Value { get; }

    public CharacterToken(Location? location, char value) : 
      base(location) {
      Value = value;
    }

    public override string ToString() {
      return $"CHARACTER '{Value}'";
    }
  }
}
