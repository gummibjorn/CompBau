﻿using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;

namespace RappiSharp.Compiler.Parser
{
    internal sealed class RappiParser
    {
        private readonly RappiLexer _lexer;
        private Token _current;

        public RappiParser(RappiLexer lexer)
        {
            _lexer = lexer;
            Next();
        }

        public void ParseProgram()
        {
            while (!IsEnd())
            {
                ParseClass();
            }
        }

        private void ParseClass()
        {
            Check(Tag.Class);
            ReadIdentifier();
            Check(Tag.OpenBrace);
            while (!IsEnd() && !Is(Tag.CloseBrace))
            {
                ParseClassMember();
            }
            Check(Tag.CloseBrace);
        }

        private void ParseClassMember()
        {
            ParseType();
            var identifier = ReadIdentifier();
            if (Is(Tag.OpenParenthesis))
            {
                ParseMethodRest(identifier);
            }
            else
            {
                Check(Tag.Semicolon);
            }
        }

        private void ParseMethodRest(string identifier)
        {
            ParseParameterList();
            ParseStatementBlock();
        }

        private void ParseParameterList()
        {
            Check(Tag.OpenParenthesis);
            if (IsIdentifier())
            {
                ParseParameter();
                while (Is(Tag.Comma))
                {
                    Next();
                    ParseParameter();
                }
            }
            Check(Tag.CloseParenthesis);
        }

        private void ParseParameter()
        {
            ParseType();
            ReadIdentifier();
        }

        private void ParseType()
        {
            ReadIdentifier();
            while (Is(Tag.OpenBracket))
            {
                Next();
                Check(Tag.CloseBracket);
            }
        }

        private void ParseStatementBlock()
        {
            Check(Tag.OpenBrace);
            while (!IsEnd() && !Is(Tag.CloseBrace))
            {
                ParseStatement();
            }
            Check(Tag.CloseBrace);
        }

        private void ParseStatement()
        {
            if (Is(Tag.If))
            {
                ParseIfStatement();
            }
            else if (Is(Tag.While))
            {
                ParseWhileStatement();
            }
            else if (Is(Tag.Return))
            {
                ParseReturnStatement();
            }
            else if (IsIdentifier())
            {
                ParseBasicStatement();
            }
            else if (Is(Tag.Semicolon))
            {
                Next();
            }
            else if (!Is(Tag.CloseBrace))
            {
                Error($"Invalid statement {_current}");
                Next();
            }
        }

        private void ParseIfStatement()
        {
            Check(Tag.If);
            Check(Tag.OpenParenthesis);
            ParseExpression();
            Check(Tag.CloseParenthesis);
            ParseStatementBlock();
            if (Is(Tag.Else))
            {
                Next();
                ParseStatementBlock();
            }
        }

        private void ParseWhileStatement()
        {
            Check(Tag.While);
            Check(Tag.OpenParenthesis);
            ParseExpression();
            Check(Tag.CloseParenthesis);
            ParseStatementBlock();
        }

        private void ParseReturnStatement()
        {
            Check(Tag.Return);
            if (Is(Tag.Semicolon))
            {
                //void return
            } else {
                ParseExpression();
            }
        }

        private void ParseBasicStatement()
        {
            var id = ReadIdentifier();
            if (IsIdentifier()) //local variable declaration
            {
                ReadIdentifier();
                Check(Tag.Semicolon);
            } else if (Is(Tag.Equals)) //assignment
            {
                Check(Tag.Assign);
                ParseExpression();
            } else if (Is(Tag.OpenParenthesis)) //method call
            {
                ParseMethodCallRest(id);

            } else
            {
                Error("Expected identifier, '=' or '('");
            }

            // TODO: Parse local variable declaration, assignment, or method call (ambigious FIRST)
        }

        private void ParseDesignatorRest(string identifier)
        {
            while (Is(Tag.Period))
            {
                Next();
                ReadIdentifier();
            }
            if (Is(Tag.OpenBracket))
            {
                Next();
                ParseExpression();
                Check(Tag.CloseBracket);
            }
        }

        private void ParseMethodCallRest(string identifier)
        {
            ParseDesignatorRest(identifier);
            ParseArgumentList();
            Check(Tag.Semicolon);
        }

        private void ParseArgumentList()
        {
            Check(Tag.OpenParenthesis);
            if (IsIdentifier())
            {
                ParseExpression();
                while (Is(Tag.Comma))
                {
                    ParseExpression();
                }
            }
        }


        private void Next()
        {
            _current = _lexer.Next();
        }

        private bool Is(Tag tag)
        {
            return _current is FixToken && ((FixToken)_current).Tag == tag;
        }

        private bool IsEnd()
        {
            return Is(Tag.End);
        }

        private void Check(Tag tag)
        {
            if (!Is(tag))
            {
                Error($"{tag} expected");
            }
            Next();
        }

        private bool IsIdentifier()
        {
            return _current is IdentifierToken;
        }

        private string ReadIdentifier()
        {
            if (!IsIdentifier())
            {
                Error("Identifier expected");
                Next();
                return "$ERROR";
            }
            var name = ((IdentifierToken)_current).Name;
            Next();
            return name;
        }

        private bool IsInteger()
        {
            return _current is IntegerToken;
        }

        private long ReadInteger(bool allowMinValue)
        {
            if (!IsInteger())
            {
                Error("Integer expected");
                Next();
                return default(int);
            }
            var value = ((IntegerToken)_current).Value;
            Next();
            return value;
        }

        private bool IsCharacter()
        {
            return _current is CharacterToken;
        }

        private char ReadCharacter()
        {
            if (!IsCharacter())
            {
                Error("Character expected");
                Next();
                return default(char);
            }
            var value = ((CharacterToken)_current).Value;
            Next();
            return value;
        }

        private bool IsString()
        {
            return _current is StringToken;
        }

        private string ReadString()
        {
            if (!IsString())
            {
                Error("String expected");
                Next();
                return string.Empty;
            }
            var value = ((StringToken)_current).Value;
            Next();
            return value;
        }

        private void Error(string message)
        {
            Location location = (Location)(_current.Location.HasValue ? _current.Location : new Location(-1, -1));
          Diagnosis.ReportError(location, message);
        }

        private void ParseExpression() {
            ReadInteger(false);
        }
    }
}