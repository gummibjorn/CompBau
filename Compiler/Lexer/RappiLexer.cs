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

        private int _col = 0, _row = 0;

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
                //...
                default: return reportError(CurrentLocation(), $"Char '{_current}' is not allowed");
            }
        }

        private ErrorToken reportError(Location location, string message)
        {
            Diagnosis.ReportError(message);
            return new ErrorToken(location, message);
        }

        private Token ReadInteger()
        {
            uint value = 0;
            uint old_value = 0;
            var location = CurrentLocation();
            while (!_endOfText && IsDigit(_current))
            {
                old_value = value;
                uint digit = (uint)_current - '0';
                value = value * 10 + digit;
                if(value < old_value)
                {
                    return reportError(location, "32bit unsigned int overflow");
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


        StringToken ReadString()
        {
            ReadNext(); // skip beginning double quote 
            string value = "";
            while (!_endOfText && _current != '"')
            {
                value += _current;
                ReadNext();
            }
            if (_endOfText)
            {
                // Error: String not closed
            }
            ReadNext(); // skip ending double quote return new StringToken(value);
            return null; //REMOVE THIS
        }
    }
}
