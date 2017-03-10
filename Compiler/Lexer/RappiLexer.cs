using RappiSharp.Compiler.Lexer.Tokens;
using System;
using System.Collections.Generic;
using System.IO;

namespace RappiSharp.Compiler.Lexer
{
    internal sealed class RappiLexer
    {
        private readonly TextReader _reader;
        private char _current;
        private bool _endOfText;

        private int _col = 0, _row = 1;

        private readonly Dictionary<String, Tag> _keywords = new Dictionary<string, Tag>()
        {
            { "class", Tag.Class },
            { "else", Tag.Else },
            { "if", Tag.If },
            { "is", Tag.Is },
            { "new", Tag.New },
            { "return", Tag.Return },
            { "while", Tag.While }
        };
        
        private readonly Dictionary<char, char> _validEscapes = new Dictionary<char, char>() {
            { 'n', '\n' },
            { '\'', '\'' },
            { '"', '"' },
            { '\\', '\\' },
            { '0', '\0' }
        };

        public RappiLexer(TextReader reader)
        {
            _reader = reader;
        }

        private void ReadNext()
        {
            var tmp = _reader.Read();
            if (tmp == -1)
            {
                _endOfText = true;
                UpdateLocation('1');
            }
            else
            {
                _current = (char)tmp;
                UpdateLocation(_current);
            }
        }

        private void UpdateLocation(char current)
        {
            if(current == '\n')
            {
                _row += 1;
                _col = 0;
            } else
            {
                _col += 1;
            }
        }

        private Location CurrentLocation()
        {
            return new Location(_row, _col);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        private void SkipBlanks()
        {
            while (!_endOfText && _current <= ' ')
            {
                ReadNext();
            }
        }

        public Token Next()
        {
            SkipBlanks();
            var location = CurrentLocation();
            if (_endOfText) { return new FixToken(CurrentLocation(), Tag.End); }
            if (IsDigit(_current)) { return ReadInteger(); }
            if (IsLetter(_current)) { return ReadName(); }

            Func<Tag, Token> fixToken = (Tag t) => { ReadNext(); return new FixToken(location, t); };
            Func<char, Tag, Tag, Token> fixToken_1or2Sequence = (char next, Tag tag1, Tag tag2) =>
            {
                    ReadNext();
                    if (!_endOfText && _current == next) { ReadNext();  return new FixToken(location, tag2); }
                    else { return new FixToken(location, tag1); }
            };
            Func<char, Tag, Token> fixToken_2Sequence = (char next, Tag t) =>
            {
                ReadNext();
                if (!_endOfText && _current == next) { ReadNext(); return new FixToken(location, t); }
                else { return reportError(CurrentLocation(), $"Invalid sequence {next}{_current}"); }

            };

            switch (_current)
            {
                case '"': return ReadString();
                case '\'': return ReadChar();
                case '/': return ReadSlash();
                case '+': return fixToken(Tag.Plus);
                case '-': return fixToken(Tag.Minus);
                case '*': return fixToken(Tag.Times);
                case '%': return fixToken(Tag.Modulo);
                case '=': return fixToken_1or2Sequence('=', Tag.Assign, Tag.Equals);
                case '!': return fixToken_1or2Sequence('=', Tag.Not, Tag.Unequal);
                case '<': return fixToken_1or2Sequence('=', Tag.Less, Tag.LessEqual);
                case '>': return fixToken_1or2Sequence('=', Tag.Greater, Tag.GreaterEqual);
                case '&': return fixToken_2Sequence('&', Tag.And);
                case '|': return fixToken_2Sequence('|', Tag.Or);
                case '{': return fixToken(Tag.OpenBrace);
                case '}': return fixToken(Tag.CloseBrace);
                case '[': return fixToken(Tag.OpenBracket);
                case ']': return fixToken(Tag.CloseBracket);
                case '(': return fixToken(Tag.OpenParenthesis);
                case ')': return fixToken(Tag.CloseParenthesis);
                case ':': return fixToken(Tag.Colon);
                case ',': return fixToken(Tag.Comma);
                case '.': return fixToken(Tag.Period);
                case ';': return fixToken(Tag.Semicolon);
                default:
                    ReadNext();
                    return reportError(location, $"Char '{_current}' is not allowed");
            }
        }

        private Token ReadChar()
        {
            var location = CurrentLocation();
            ReadNext();

            Func<Token, Token> makeTokenChecked = (Token output) =>
            {
                ReadNext();
                if (_current != '\'') { return reportError(location, "Char literal too long"); }
                ReadNext();
                return output;
            };

            if (_current == '\\')
            {
                ReadNext();
                if (_validEscapes.ContainsKey(_current))
                {
                    return makeTokenChecked(new CharacterToken(location, _validEscapes[_current]));
                }
                else
                {
                    return makeTokenChecked(reportError(location, $"Invalid escape code: \\{_current}"));
                }
            }
            if (_endOfText) { return new FixToken(location, Tag.End); }
            if (_current == '\'') { return reportError(location, "Char literal cannot be empty"); }
            else { return makeTokenChecked(new CharacterToken(location, _current)); }
        }

        private Token ReadSlash()
        {
            ReadNext();
            if (_current != '/' && _current != '*')
            {
                return new FixToken(CurrentLocation(), Tag.Divide);
            }
            SkipComment();
            return Next();
        }

        private void SkipComment()
        {
            if(_current == '/')
            {
                while (_current != '\n' && !_endOfText)
                    ReadNext();
                    
            }else if (_current == '*')
            {
                while (_current != '/')
                {
                    do
                    {
                        ReadNext();
                    } while (_current != '*');
                    ReadNext();
                }
                ReadNext();
            }
        }

        private ErrorToken reportError(Location location, string message)
        {
            Diagnosis.ReportError(location, message);
            return new ErrorToken(location, message);
        }

        private Token ReadInteger()
        {
            long value = 0;
            long maxAllowedValue = int.MinValue;
            var location = CurrentLocation();
            while (!_endOfText && IsDigit(_current))
            {
                int digit = _current - '0';
                value = value * 10 + digit;
                if(value > Math.Abs(maxAllowedValue))
                {
                    return reportError(location, "32bit int overflow");
                }
                ReadNext();
            }
            return new IntegerToken(location, value);
        }

        Token ReadName()
        {
            var location = CurrentLocation();
            string name = _current.ToString();
            ReadNext();
            while (!_endOfText &&
          (IsLetter(_current) || IsDigit(_current)))
            {
                name += _current;
                ReadNext();
            }
            if (_keywords.ContainsKey(name))
            {
                return new FixToken(location, _keywords[name]);
            }
            return new IdentifierToken(location, name);
        }

        private void jumpToEndOfString()
        {
            while (!_endOfText && _current != '"')
            {
                ReadNext();
            }
            ReadNext(); // skip ending double quote 
        }


        private Token ReadString()
        {
            var location = CurrentLocation();
            ReadNext(); // skip beginning double quote 
            string value = "";
            while (!_endOfText && _current != '"')
            {
                if (_current == '\\'){
                    ReadNext();
                    if (_validEscapes.ContainsKey(_current)){
                        value += _validEscapes[_current];
                    } else { 
                        var invalidEscape = _current;
                        jumpToEndOfString();
                        return reportError(location, $"Invalid escape code: {invalidEscape}");
                    }
                } else {
                    value += _current;
                }
                ReadNext();
            }
            if (_endOfText)
            {
                return reportError(location, "Unterminated String");
            }
            ReadNext(); // skip ending double quote return new StringToken(value);
            return new StringToken(location, value);
        }
    }
}
