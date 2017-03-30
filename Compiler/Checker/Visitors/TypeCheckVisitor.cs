using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System;

namespace RappiSharp.Compiler.Checker.Visitors
{
    internal class TypeCheckVisitor : Visitor
    {
        private readonly SymbolTable _symbolTable;
        private readonly MethodSymbol _method;

        public TypeCheckVisitor(SymbolTable symbolTable, MethodSymbol method)
        {
            _symbolTable = symbolTable;
            _method = method;
        }

        public override void Visit(UnaryExpressionNode node)
        {
            var optr = node.Operator;
            var operand = node.Operand;
            var type = _symbolTable.FindType(operand);
            switch (optr)
            {
                case Operator.Not:
                    if (type == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, $"Invalid type {type.ToString()} for unary '!'");
                    }
                    break;
                case Operator.Plus:
                    checkIntegerMaxValue(operand);
                    if (type == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, $"Invalid type {type.ToString()} for unary {optr.ToString()}");
                    }
                    break;
                case Operator.Minus:
                    if (type == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, $"Invalid type {type.ToString()} for unary {optr.ToString()}");
                    }
                    break;
            }

            base.Visit(node);
        }

        private void checkIntegerMaxValue(ExpressionNode node)
        {
            if (node is IntegerLiteralNode)
            {
                if (((IntegerLiteralNode)node).Value > int.MaxValue)
                {
                    Diagnosis.ReportError(node.Location, "Integer maxvalue exceeded");
                }
            }
        }

        public override void Visit(IntegerLiteralNode node)
        {
            _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
        }

        public override void Visit(BinaryExpressionNode node)
        {
            base.Visit(node);

            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);

            checkIntegerMaxValue(node.Left);
            checkIntegerMaxValue(node.Right);

            switch (node.Operator)
            {
                case Operator.Or:
                case Operator.And:
                    if (leftType == _symbolTable.Compilation.BoolType && rightType == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, "comparing apples and oranges gives you scurvies");

                        throw new System.Exception($"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}");
                    }
                    break;
                case Operator.Divide:
                case Operator.Times:
                case Operator.Modulo:
                case Operator.Minus:
                case Operator.Plus:
                    if (leftType == _symbolTable.Compilation.IntType && rightType == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, "Invalid types in binary expression");
                        throw new System.Exception($"Wrong type in binary expression '{leftType?.ToString()}' '{node.Operator}' '{rightType?.ToString()}'");
                    }
                    break;
                case Operator.Less:
                case Operator.LessEqual:
                case Operator.Greater:
                case Operator.GreaterEqual:
                    if (leftType == _symbolTable.Compilation.IntType && rightType == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, "Invalid types in binary expression");
                        throw new System.Exception($"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}");
                    }
                    break;
                case Operator.Equals:
                case Operator.Unequal:
                case Operator.Is:
                    if (leftType.Identifier == rightType.Identifier)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, "Invalid types in binary expression");
                        throw new System.Exception($"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}");
                    }
                    break;
            }
        }

        private void checkNotLength(DesignatorNode node)
        {
            if (node is BasicDesignatorNode)
            {
                if(((BasicDesignatorNode)node).Identifier == "length")
                {
                    Diagnosis.ReportError(node.Location, "Length must not be on the left side of an assignment");
                    throw new Exception("Length must not be on the left side of assignment");
                }
            }else
            {
                checkNotLength(((MemberAccessNode)node).Designator);
            }
        }

        // TODO: Implement type checks for entire program
        public override void Visit(AssignmentNode node)
        {
            base.Visit(node);
            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);
            checkIntegerMaxValue(node.Right);
            checkNotLength(node.Left);
            //TODO: length must not be on left side!
            if (leftType != rightType)
            {
                Diagnosis.ReportError(node.Location, $"Cannot assign '{rightType?.ToString()}' to '{leftType?.ToString()}'");
            }
        }

        public override void Visit(MethodCallNode node)
        {
            base.Visit(node);
            var methodSymbol = _symbolTable.GetTarget(node.Designator);
            var methodDefinition = _symbolTable.GetDeclarationNode<MethodNode>(methodSymbol);
            var returnType = _symbolTable.FindType(methodDefinition.ReturnType);

            if (methodDefinition.Parameters.Count != node.Arguments.Count)
            {
                Diagnosis.ReportError(node.Location, $"Invalid argument count");
                return;
            }
            for (var i = 0; i < node.Arguments.Count; i += 1)
            {
                var param = methodDefinition.Parameters[i];
                var paramType = _symbolTable.FindType(param.Type);
                var arg = node.Arguments[i];
                var argType = _symbolTable.FindType(arg);
                if (paramType != argType)
                {
                    Diagnosis.ReportError(arg.Location, $"Cannot assign argument of type {argType} to parameter {param.Identifier} ({paramType})");
                }
            }

            _symbolTable.FixType(node, returnType);
        }

        public override void Visit(ReturnStatementNode node)
        {
            base.Visit(node);

            var returnType = _symbolTable.FindType(node.Expression);
            var methodReturnType = _method.ReturnType.Identifier;

            if (returnType.Identifier != methodReturnType)
            {
                Diagnosis.ReportError(node.Location, $"Method return type {methodReturnType} does not match return expression type {returnType.Identifier}");
                throw new Exception($"Method return type {methodReturnType} does not match return expression type {returnType.Identifier}");
            }
        }

        private void checkBoolCondition(ExpressionNode condition)
        {
            var type = _symbolTable.FindType(condition);
            if(type != _symbolTable.Compilation.BoolType)
            {
                Diagnosis.ReportError(condition.Location, $"Condition must be of type bool");
            }
        }

        public override void Visit(IfStatementNode node)
        {
            base.Visit(node);
            checkBoolCondition(node.Condition);
        }

        public override void Visit(WhileStatementNode node)
        {
            base.Visit(node);
            checkBoolCondition(node.Condition);
        }

        public override void Visit(ObjectCreationNode node)
        {
            base.Visit(node);
            var type = _symbolTable.FindType(node.Type);
            _symbolTable.FixType(node, type);
        }

        public override void Visit(ArrayCreationNode node)
        {
            base.Visit(node);
            var arrayType = _symbolTable.FindType(new ArrayTypeNode(node.Location, node.ElementType));
            var expressionType = _symbolTable.FindType(node.Expression);

            if(expressionType != _symbolTable.Compilation.IntType)
            {
                Diagnosis.ReportError(node.Location, "Invalid expression type in array creation");

            }

            _symbolTable.FixType(node, arrayType);

        }
    }
}
