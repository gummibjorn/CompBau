namespace RappiSharp.Compiler.Lexer.Tokens {
  internal enum Tag {
    // specials
    End,
    // keywords
    Class,
    Else,
    If,
    Is,
    New,
    Return,
    While,
    // operators
    And,
    Assign,
    Divide,
    Equals, Unequal,
    Greater, GreaterEqual,
    Minus,
    Modulo,
    Not,
    Or,
    Plus,
    Less, LessEqual,
    Times,
    // interpunction
    OpenBrace, CloseBrace,
    OpenBracket, CloseBracket,
    OpenParenthesis, CloseParenthesis,
    Colon,
    Comma,
    Period,
    Semicolon
  }
}
