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

        private Location LOCATION = new Location(0, 0);

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
            }
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
                return new FixToken(LOCATION, Tag.End);
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
                case '+': ReadNext();  return new FixToken(LOCATION, Tag.Plus);
                //...
                default: return null;
            }
        }


        private IntegerToken ReadInteger()
        {
            int value = 0;
            while (!_endOfText && IsDigit(_current))
            {
                int digit = _current - '0';
                value = value * 10 + digit;
                ReadNext();
            }
            return new IntegerToken(LOCATION, value);
            //TODO: handle overflows
        }

        private Dictionary<String, Tag> _keywords;
        Token ReadName()
        {
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
                return new FixToken(LOCATION, _keywords[name]);
            }
            return new IdentifierToken(LOCATION, name);
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
