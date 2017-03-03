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

        private Dictionary<String, Tag> _keywords = new Dictionary<string, Tag>()
        {
            { "class", Tag.Class },
            { "else", Tag.Else },
            { "if", Tag.If },
            { "is", Tag.Is },
            { "new", Tag.New },
            { "return", Tag.Return },
            { "while", Tag.While }
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
            return c >= 'A' && c <= 'z';
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
            if (_endOfText)
            {
                return new FixToken(CurrentLocation(), Tag.End);
            }
            if (IsDigit(_current))
            {
                return ReadInteger();
            }
            if (IsLetter(_current))
            {
                return ReadName();
            }
            switch (_current)
            {
                case '"': return ReadString();
                case '+': ReadNext();  return new FixToken(CurrentLocation(), Tag.Plus);
                case '/': return ReadSlash();
                //...
                default: return reportError(CurrentLocation(), $"Char '{_current}' is not allowed");
            }
        }

        private Token ReadSlash()
        {
            ReadNext();
            if(_current != '/' && _current != '*')
            {
                return new FixToken(CurrentLocation(), Tag.Divide);
            }else if(_current == '/')
            {
                while (_current != '\n' && !_endOfText)
                    ReadNext();
                    
            }else if (_current == '*')
            {
                while (_current != '/')
                {
                    ReadCommentBlock();
                }
                ReadNext();
            }
            return Next();
        }

        private void ReadCommentBlock()
        {
                do
                {
                    ReadNext();
                } while (_current != '*');
                ReadNext();
        }

        private ErrorToken reportError(Location location, string message)
        {
            Diagnosis.ReportError(message);
            return new ErrorToken(location, message);
        }

        private Token ReadInteger()
        {
            int value = 0;
            int old_value = 0;
            var location = CurrentLocation();
            while (!_endOfText && IsDigit(_current))
            {
                old_value = value;
                int digit = _current - '0';
                value = value * 10 + digit;
                if(value < old_value)
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


        Token ReadString()
        {
            var location = CurrentLocation();
            ReadNext(); // skip beginning double quote 
            string value = "";
            while (!_endOfText && _current != '"')
            {
                value += _current;
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
