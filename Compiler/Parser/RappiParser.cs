using System;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System.Collections.Generic;

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
            
        }

        private void ParseWhileStatement()
        {
            // TODO: Implement
        }

        private void ParseReturnStatement()
        {
            // TODO: Implement
        }

        private void ParseExpression()
        {
            ParseLogicTerm();
            while (Is(Tag.Or))
            {
                ParseLogicTerm();
            }
        }

        private void ParseLogicTerm()
        {
            ParseLogicFactor();
            while (Is(Tag.Or))
            {
                ParseLogicFactor();
            }
        }

        private void ParseLogicFactor()
        {
            ParseSimpleExpression();
            while (IsCompareOperator()){
                ParseSimpleExpression();
            }
        }

        private void ParseSimpleExpression()
        {
            ParseTerm();
            while (Is(Tag.Plus) || Is(Tag.Minus))
            {
                ParseTerm();
            }
        }

        private void ParseTerm()
        {
            ParseFactor();
            while(Is(Tag.Times) || Is(Tag.Divide) || Is(Tag.Modulo))
            {
                ParseFactor();
            }
        }

        private void ParseFactor()
        {
            if(IsInteger() || IsString() || IsCharacter())
            {
                ParseOperand();
            }else if (Is(Tag.Not) || Is(Tag.Plus) || Is(Tag.Minus))
            {
                ParseUnaryExpression(); 
            }else if (Is(Tag.OpenParenthesis))
            {
                //TODO: Implement and add ParseTypeCast here!
                Next();
                ParseExpression();
                if (!Is(Tag.CloseParenthesis))
                {
                    Error($"Invalid Statement {_current}");
                }
                Next(); 
            }
        }

        private void ParseUnaryExpression()
        {
            Next();
            ParseFactor();
        }

        private void ParseOperand()
        {
            if (IsInteger())
            {
                ReadInteger(true);
            }else if (IsIdentifier())
            {
                //ParseDesignator();
            }else if (Is(Tag.New)){
                Next();
                //TODO Implement Object and Array Creation
            }
        }

        private void ParseCompareOperator()
        {
            if (Is(Tag.Equals))
            {

            }else if (Is(Tag.Unequal))
            {
                
            }else if (Is(Tag.Less))
            {

            }else if (Is(Tag.LessEqual))
            {

            }else if (Is(Tag.Greater))
            {

            }else if (Is(Tag.GreaterEqual))
            {

            }else if (Is(Tag.Is))
            {

            }
            Next();
        }

        private void ParseBasicStatement()
        {
            // TODO: Parse local variable declaration, assignment, or method call (ambigious FIRST)
        }

        // TODO: Continue parser implementation    

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

        private bool IsCompareOperator()
        {
            var compareTags = new List<Tag>
            {
                Tag.Equals,
                Tag.Not,
                Tag.Less,
                Tag.LessEqual,
                Tag.Greater,
                Tag.GreaterEqual,
                Tag.Is
            };
            return _current is FixToken && compareTags.Contains(((FixToken)_current).Tag);
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
    }
}
