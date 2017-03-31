using System;
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
            BasicTypeNode baseClass = null;
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
                } else
                {
                    methods.Add((MethodNode)classMember);
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
                return ParseMethodRest(type, identifier);
            }
            else
            {
                Check(Tag.Semicolon);
                return new VariableNode(currentLocation, type, identifier);
            }
        }

        private MethodNode ParseMethodRest(TypeNode type, string identifier)
        {
            var location = CurrentLocation();
            var parameters = ParseParameterList();
            var body = ParseStatementBlock();
            return new MethodNode(location, type, identifier, parameters, body);
        }

        private List<VariableNode> ParseParameterList()
        {
            var list = new List<VariableNode>();
            Check(Tag.OpenParenthesis);
            if (IsIdentifier())
            {
                list.Add(ParseParameter());
                while (Is(Tag.Comma))
                {
                    Next();
                    list.Add(ParseParameter());
                }
            }
            Check(Tag.CloseParenthesis);
            return list;
        }

        private VariableNode ParseParameter()
        {
            var location = CurrentLocation();
            var type = ParseType();
            var id = ReadIdentifier();
            return new VariableNode(location, type, id);
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

        private TypeNode ParseTypeRest(Location location, string identifier)
        {
            if (!Is(Tag.OpenBracket))
            {
                return new BasicTypeNode(location, identifier);
            }
            return ArrayTypeBuilder(location, identifier);
        }

        private TypeNode ParseType()
        {
            var currentLocation = CurrentLocation();
            var identifier = ReadIdentifier();
            return ParseTypeRest(currentLocation, identifier);
        }

        private StatementBlockNode ParseStatementBlock()
        {
            var location = CurrentLocation();
            var list = new List<StatementNode>();
            Check(Tag.OpenBrace);
            while (!IsEnd() && !Is(Tag.CloseBrace))
            {
                var statement = ParseStatement();
                if(statement != null)
                {
                    list.Add(statement);
                }
            }
            Check(Tag.CloseBrace);
            return new StatementBlockNode(location, list);
        }

        private StatementNode ParseStatement()
        {
            if (Is(Tag.If))
            {
                return ParseIfStatement();
            }
            else if (Is(Tag.While))
            {
                return ParseWhileStatement();
            }
            else if (Is(Tag.Return))
            {
                return ParseReturnStatement();
            }
            else if (IsIdentifier())
            {
                return ParseBasicStatement();
            }
            else if (Is(Tag.Semicolon))
            {
                Next();
                return null;
            }
            //hummm?
            else if (!Is(Tag.CloseBrace))
            {
                Error($"Invalid statement {_current}");
                Next();
                return null;
            }
            Error("Invalid statement");
            return null;
        }


        private IfStatementNode ParseIfStatement()
        {
            var location = CurrentLocation();
            Check(Tag.If);
            Check(Tag.OpenParenthesis);
            var condition = ParseExpression();
            Check(Tag.CloseParenthesis);
            var thenBody = ParseStatementBlock();
            StatementBlockNode elseBody = null;
            if (Is(Tag.Else))
            {
                Next();
                elseBody = ParseStatementBlock();
            }
            return new IfStatementNode(location, condition, thenBody, elseBody);
            
        }

        private WhileStatementNode ParseWhileStatement()
        {
            var location = CurrentLocation();
            Check(Tag.While);
            Check(Tag.OpenParenthesis);
            var condition = ParseExpression();
            Check(Tag.CloseParenthesis);
            var body = ParseStatementBlock();
            return new WhileStatementNode(location, condition, body);
        }

        private ReturnStatementNode ParseReturnStatement()
        {
            var location = CurrentLocation();
            Check(Tag.Return);
            if (Is(Tag.Semicolon))
            {
                Next();
                return new ReturnStatementNode(location, null);
            } else {
                var expression = ParseExpression();
                Check(Tag.Semicolon);
                return new ReturnStatementNode(location, expression);
            }
        }

        private ExpressionNode ParseExpression(String ident = null)
        {
            var location = CurrentLocation();
            var left = ParseLogicTerm(ident);
            while (Is(Tag.Or))
            {
                var op = Operator.Or;
                Next();
                var right = ParseLogicTerm();
                left = new BinaryExpressionNode(location, left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseLogicTerm(String ident = null)
        {
            var location = CurrentLocation();
            var left = ParseLogicFactor(ident);
            while (Is(Tag.And))
            {
                var op = Operator.And;
                Next();
                var right = ParseLogicFactor();
                left = new BinaryExpressionNode(location, left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseLogicFactor(String ident = null)
        {
            var location = CurrentLocation();
            var left = ParseSimpleExpression(ident);
            while (IsCompareOperator()){
                var op = ParseCompareOperator();
                var right = ParseSimpleExpression();
                left = new BinaryExpressionNode(location, left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseSimpleExpression(String ident = null)
        {
            var location = CurrentLocation();
            var left = ParseTerm(ident);
            while (Is(Tag.Plus) || Is(Tag.Minus))
            {
                var op = Is(Tag.Plus) ? Operator.Plus : Operator.Minus;
                Next();
                var right = ParseTerm();
                left = new BinaryExpressionNode(location, left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseTerm(String ident = null)
        {
            var location = CurrentLocation();
            var left = ParseFactor(ident);
            while(Is(Tag.Times) || Is(Tag.Divide) || Is(Tag.Modulo))
            {
                var op = Is(Tag.Times) ? Operator.Times : Is(Tag.Divide) ? Operator.Divide : Operator.Modulo;
                Next();
                var right = ParseFactor();
                left = new BinaryExpressionNode(location, left, op, right);
            }
            return left;
        }

        private ExpressionNode ParseFactor(String identPassthrough = null)
        {
            if (Is(Tag.Not) || Is(Tag.Plus) || Is(Tag.Minus))
            {
                return ParseUnaryExpression(); 
            }else if (Is(Tag.OpenParenthesis))
            {
                Next();
                if (IsIdentifier())
                {
                    var ident = ReadIdentifier();

                    //FIX Bug (identifier);
                    if (Is(Tag.CloseParenthesis))
                    {
                        Next();
                        return new TypeCastNode(CurrentLocation(), new BasicTypeNode(CurrentLocation(), ident), ParseDesignatorRest(new BasicDesignatorNode(CurrentLocation(), ReadIdentifier())));
                    }else
                    {
                        var expr = ParseExpression(ident);
                        Check(Tag.CloseParenthesis);
                        return expr;
                    }
                }
                else
                {
                    var expr = ParseExpression();
                    Check(Tag.CloseParenthesis);
                return expr;
                }
            }else 
            {
                return ParseOperand(identPassthrough);
            }
        }

        private UnaryExpressionNode ParseUnaryExpression()
        {
            var location = CurrentLocation();
            var op = Is(Tag.Not) ? Operator.Not : Is(Tag.Plus) ? Operator.Plus : Operator.Minus;
            Next();
            var factor = ParseFactor();
            return new UnaryExpressionNode(location, op, factor);
        }

        private ExpressionNode ParseOperand(String identPassthrough = null)
        {
            var location = CurrentLocation();
            if (IsInteger())
            {
                return new IntegerLiteralNode(CurrentLocation(), ReadInteger(true));
            }else if (IsCharacter())
            {
                return new CharacterLiteralNode(CurrentLocation(), ReadCharacter());    
            }else if (IsString())
            {
                return new StringLiteralNode(CurrentLocation(), ReadString());
            }else if (IsIdentifier() || identPassthrough != null)
            {
                var identifier = identPassthrough == null ? ReadIdentifier() : identPassthrough;
                var designator = ParseDesignatorRest(new BasicDesignatorNode(location, identifier));
                if (Is(Tag.OpenParenthesis))
                {
                    return ParseMethodCallRest(location, designator);
                } else
                {
                    return designator;
                }
            }else if (Is(Tag.New)){
                Next();
                var ident = ReadIdentifier();
                if (Is(Tag.OpenParenthesis))
                {
                    Check(Tag.OpenParenthesis);
                    Check(Tag.CloseParenthesis);
                    return new ObjectCreationNode(location, new BasicTypeNode(CurrentLocation(), ident));
                }else if (Is(Tag.OpenBracket))
                {
                    return ParseArrayCreation(ident, location);
                }else
                {
                    Error("Invalid symbol in creation expression: " + _current);
                    return null;
                }
            }else
            {
                Error("Invalid operand: " + _current);
                return null;
            }
        }

        private ArrayCreationNode ParseArrayCreation(String identifier, Location location)
        {
            Check(Tag.OpenBracket);
            var expr = ParseExpression();
            Check(Tag.CloseBracket);
            TypeNode innerType = new BasicTypeNode(location, identifier);
            while (Is(Tag.OpenBracket))
            {
                var innerBracketLocation = CurrentLocation();
                Check(Tag.OpenBracket);
                Check(Tag.CloseBracket);
                innerType = new ArrayTypeNode(innerBracketLocation, innerType);
            }
            return new ArrayCreationNode(location, innerType , expr);
        }

        private Operator? ParseCompareOperator()
        {
            Operator? op;
            if (Is(Tag.Equals))
            {
                op = Operator.Equals;
            }
            else if (Is(Tag.Unequal))
            {
                op = Operator.Unequal;
            }
            else if (Is(Tag.Less))
            {
                op = Operator.Less;
            }
            else if (Is(Tag.LessEqual))
            {
                op = Operator.LessEqual;
            }
            else if (Is(Tag.Greater))
            {
                op = Operator.Greater;
            }
            else if (Is(Tag.GreaterEqual))
            {
                op = Operator.GreaterEqual;
            }
            else if (Is(Tag.Is))
            {
                op = Operator.Is;
            }else
            {
                Error("Invalid operator");
                op = null;
            }
            Next();
            return op;
        }

        private StatementNode ParseBasicStatement()
        {
            var location = CurrentLocation();
            var id = ReadIdentifier();
            if (IsIdentifier())
            {
                var type = new BasicTypeNode(location, id);
                var name = ReadIdentifier();
                Check(Tag.Semicolon);
                return new LocalDeclarationNode(location, new VariableNode(location, type, name));
            } else if (Is(Tag.OpenBracket))
            {
                Next();
                if (Is(Tag.CloseBracket))
                {
                    Next();
                    var type = new ArrayTypeNode(location, ArrayTypeBuilder(location, id));
                    var name = ReadIdentifier();
                    Check(Tag.Semicolon);
                    return new LocalDeclarationNode(location, new VariableNode(location, type, name));
                } else
                {
                    //Parse expression; parse designator rest
                    var index = ParseExpression();
                    Check(Tag.CloseBracket);
                    var designator = new ElementAccessNode(
                        location,
                        ParseDesignatorRest(new BasicDesignatorNode(location, id)),
                        index
                    );
                    return ParseBasicStatementRest(location, designator);
                }
            } else {
                var designator = ParseDesignatorRest(new BasicDesignatorNode(location,id));
                return ParseBasicStatementRest(location, designator);
            } 
            /*
            else
            {
                Error("Expected identifier, '=' or '('");
                return null;
            }
            */
        }

        private StatementNode ParseBasicStatementRest(Location location, DesignatorNode designator)
        {
            if (Is(Tag.Assign))
            {
                Next();
                var right = ParseExpression();
                Check(Tag.Semicolon);
                return new AssignmentNode(location, designator, right);
            } else if (Is(Tag.OpenParenthesis))
            {
                var call = new CallStatementNode(location, ParseMethodCallRest(location, designator));
                Check(Tag.Semicolon);
                return call;
            } else
            {
                Error($"Expected '=' or '(', got {_current}");
                return null;
            }
        }

        private DesignatorNode ParseDesignatorRest(DesignatorNode previousName)
        {
            var location = CurrentLocation();
            if (Is(Tag.OpenBracket))
            {
                Next();
                var index = ParseExpression();
                Check(Tag.CloseBracket);
                var elementAccess = new ElementAccessNode(
                    location,
                    ParseDesignatorRest(previousName),
                    index
                );
                return ParseDesignatorRest(elementAccess);
            } else if(Is(Tag.Period))
            {
                Next();
                var name = ReadIdentifier();
                var memberAccessNode = new MemberAccessNode(
                    location,
                    ParseDesignatorRest(previousName),
                    name
                );
                return ParseDesignatorRest(memberAccessNode);
            } else
            {
                return previousName;
            }
        }

        private MethodCallNode ParseMethodCallRest(Location location, DesignatorNode designator)
        {
            return new MethodCallNode(location,
                designator,
                ParseArgumentList()
            );

        }

        private List<ExpressionNode> ParseArgumentList()
        {
            var expressions = new List<ExpressionNode>();
            Check(Tag.OpenParenthesis);
            if (!Is(Tag.CloseParenthesis))
            {
                expressions.Add(ParseExpression());
                while (Is(Tag.Comma))
                {
                    Next();
                    expressions.Add(ParseExpression());
                }
            }
            Check(Tag.CloseParenthesis);
            return expressions;
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
                Tag.Unequal,
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
