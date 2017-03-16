﻿using System;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System.Collections.Generic;
using RappiSharp.Compiler.Parser.Tree;

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

        private Location CurrentLocation()
        {
            return (Location)(_current.Location.HasValue ? _current.Location : new Location(-1, -1));
        }

        public ProgramNode ParseProgram()
        {
            var classes = new List<ClassNode>();
            var currentLocation = CurrentLocation();
            while (!IsEnd())
            {
                classes.Add(ParseClass());
            }
            return new ProgramNode(currentLocation, classes);
        }

        private ClassNode ParseClass()
        {
            var currentLocation = CurrentLocation();
            Check(Tag.Class);
            var classIdentifier = ReadIdentifier();
            var baseClass = new BasicTypeNode(currentLocation, "Object");
            var variables = new List<VariableNode>();
            var methods = new List<MethodNode>();

            if (Is(Tag.Colon))
            {
                Next();
                baseClass = new BasicTypeNode(CurrentLocation(), ReadIdentifier());
            }
            Check(Tag.OpenBrace);
            while (!IsEnd() && !Is(Tag.CloseBrace))
            {
                var classMember = ParseClassMember();
                if(classMember is VariableNode)
                {
                    variables.Add((VariableNode)classMember);
                }
            }
            Check(Tag.CloseBrace);
            return new ClassNode(currentLocation, classIdentifier, baseClass, variables, methods);
        }

        private Node ParseClassMember()
        {
            var currentLocation = CurrentLocation();
            var type = ParseType();
            var identifier = ReadIdentifier();
            if (Is(Tag.OpenParenthesis))
            {
                ParseMethodRest(identifier);
                return null;
            }
            else
            {
                Check(Tag.Semicolon);
                return new VariableNode(currentLocation, type, identifier);
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

        private TypeNode ArrayTypeBuilder(Location currentLocation, String identifier)
        {
            if (Is(Tag.OpenBracket))
            {
                Next();
                Check(Tag.CloseBracket);
                return new ArrayTypeNode(currentLocation, ArrayTypeBuilder(currentLocation, identifier));
            }else
            {
                return new BasicTypeNode(currentLocation, identifier);
            }
        }

        private TypeNode ParseType()
        {
            var currentLocation = CurrentLocation();
            var identifier = ReadIdentifier();
            if (!Is(Tag.OpenBracket))
            {
                return new BasicTypeNode(currentLocation, identifier);
            }
            return ArrayTypeBuilder(currentLocation, identifier);
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
                Next();
                //void return
            } else {
                ParseExpression();
                Check(Tag.Semicolon);
            }
        }

        private void ParseExpression()
        {
            ParseLogicTerm();
            while (Is(Tag.Or))
            {
                Next();
                ParseLogicTerm();
            }
        }

        private void ParseLogicTerm()
        {
            ParseLogicFactor();
            while (Is(Tag.And))
            {
                Next();
                ParseLogicFactor();
            }
        }

        private void ParseLogicFactor()
        {
            ParseSimpleExpression();
            while (IsCompareOperator()){
                ParseCompareOperator();
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
                Next();
                ParseFactor();
            }
        }

        private void ParseFactor()
        {
            if (Is(Tag.Not) || Is(Tag.Plus) || Is(Tag.Minus))
            {
                ParseUnaryExpression(); 
            }else if (Is(Tag.OpenParenthesis))
            {
                //TODO: Implement and add ParseTypeCast here!
                Next();
                ParseExpression();
                Check(Tag.CloseParenthesis);
            }else 
            {
                ParseOperand();
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
            }else if (IsCharacter())
            {
                ReadCharacter();    
            }else if (IsString()) {
                ReadString();
            }else if (IsIdentifier())
            {
                var identifier = ReadIdentifier();
                ParseDesignatorRest(identifier);
                if (Is(Tag.OpenParenthesis))
                {
                    ParseMethodCallRest(/*TODO: Designator Tag */);
                    //return methodcall
                } else
                {
                    //return designator
                }
            }else if (Is(Tag.New)){
                Next();
                ReadIdentifier();
                if (Is(Tag.OpenParenthesis))
                {
                    Check(Tag.OpenParenthesis);
                    Check(Tag.CloseParenthesis);
                }else if (Is(Tag.OpenBracket))
                {
                    ParseArrayCreation();
                }
            }else
            {
                Error("Invalid operand: " + _current);
            }
        }

        private void ParseArrayCreation()
        {
            Check(Tag.OpenBracket);
            ParseExpression();
            Check(Tag.CloseBracket);
            while (Is(Tag.OpenBracket))
            {
                Check(Tag.OpenBracket);
                Check(Tag.CloseBracket);
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
                Check(Tag.Semicolon);

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
            if (Is(Tag.Period)) // FIXME: laut EBNF scheint es so, als koennte hier nur nich ein identifier kommen und der Designator waere zuende.
            {
                Next();
                ReadIdentifier();
            }
        }

        private void ParseMethodCallRest(/*TODO: Designator Tag */)
        {
            ParseArgumentList();
        }

        private void ParseMethodCallRest(string identifier)
        {
            ParseDesignatorRest(identifier);
            ParseArgumentList();
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
            Check(Tag.CloseParenthesis);
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
