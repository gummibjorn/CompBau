﻿using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System;
using System.Collections.Generic;

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

        private void Error(Location location, string msg, bool fatal = false)
        {
            Diagnosis.ReportError(location, msg);
            if (fatal)
            {
                throw new CheckerException(msg);
            }
        }

        public override void Visit(UnaryExpressionNode node)
        {
            base.Visit(node);
            var optr = node.Operator;
            var operand = node.Operand;
            var type = _symbolTable.FindType(operand);
            switch (optr)
            {
                case Operator.Not:
                    if (type == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }
                    else
                    {
                        Error(node.Location, $"Invalid type {type.ToString()} for unary '!'", true);
                    }
                    break;
                case Operator.Plus:
                    CheckIntegerMaxValue(operand);
                    if (type == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    }
                    else
                    {
                        Error(node.Location, $"Invalid type {type.ToString()} for unary {optr.ToString()}");
                    }
                    break;
                case Operator.Minus:
                    if (type == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    }
                    else
                    {
                        Error(node.Location, $"Invalid type {type.ToString()} for unary {optr.ToString()}");
                    }
                    break;
            }
        }

        private void CheckIntegerMaxValue(ExpressionNode node)
        {
            if (node is IntegerLiteralNode)
            {
                if (((IntegerLiteralNode)node).Value > int.MaxValue)
                {
                    Error(node.Location, "Integer maxvalue exceeded");
                }
            }
        }

        public override void Visit(IntegerLiteralNode node)
        {
            _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
        }

        public override void Visit(CharacterLiteralNode node)
        {
            _symbolTable.FixType(node, _symbolTable.Compilation.CharType);
        }

        public override void Visit(StringLiteralNode node)
        {
            _symbolTable.FixType(node, _symbolTable.Compilation.StringType);
        }

        public override void Visit(BinaryExpressionNode node)
        {
            base.Visit(node);

            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);

            CheckIntegerMaxValue(node.Left);
            CheckIntegerMaxValue(node.Right);

            switch (node.Operator)
            {
                case Operator.Or:
                case Operator.And:
                    if (leftType == _symbolTable.Compilation.BoolType && rightType == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }
                    else
                    {
                        Error(node.Location, "comparing apples and oranges gives you scurvies", true);
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
                    }
                    else
                    {
                        Error(node.Location, $"Wrong type in binary expression '{leftType?.ToString()}' '{node.Operator}' '{rightType?.ToString()}'", true);
                    }
                    break;
                case Operator.Less:
                case Operator.LessEqual:
                case Operator.Greater:
                case Operator.GreaterEqual:
                    if (leftType == _symbolTable.Compilation.IntType && rightType == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }
                    else
                    {
                        Error(node.Location, $"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}", true);
                    }
                    break;
                case Operator.Equals:
                case Operator.Unequal:
                    bool isLeftRef = IsReferenceType(leftType);
                    bool isRightRef = IsReferenceType(rightType);
                    bool isLeftNull = leftType == _symbolTable.Compilation.NullType;
                    bool isRightNull = rightType == _symbolTable.Compilation.NullType;

                    //FIXME these exceptions don't have to be fatal - we can fix the type even if errors occur
                    if (!isLeftRef && isRightNull || !isRightRef && isLeftNull)
                    {
                        Error(node.Location, $"Wrong type in comparison {leftType.ToString()} {node.Operator} {rightType.ToString()}", true);
                    }

                    if (isAssignable(leftType, rightType) || isAssignable(rightType, leftType))
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }
                    else
                    {
                        Error(node.Location, $"Wrong type in comparison {leftType.ToString()} {node.Operator} {rightType.ToString()}", true);
                    }
                    break;
                case Operator.Is:
                    _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    if (!IsReferenceType(leftType))
                    {
                        Error(node.Location, "Left hand side of 'is' must be a reference type");
                        break;
                    }
                    if (node.Right is BasicDesignatorNode)
                    {
                        var identifier = ((BasicDesignatorNode)node.Right).Identifier;
                        var type = _symbolTable.Compilation.Classes.Find(c => c.Identifier == identifier);
                        if (type == null)
                        {
                            Error(node.Right.Location, $"Undeclared class '{identifier}'");
                        }
                        else
                        {
                            if (!isAssignable(type, leftType) && !isAssignable(leftType, type))
                            {
                                Error(node.Location, $"Left hand side of 'is' can never be of type '{leftType.Identifier}'");
                            }
                            //OK
                        }
                    }
                    else
                    {
                        Error(node.Left.Location, "Left hand side of 'is' must be a class name");
                    }
                    break;
            }
        }

        private bool IsReferenceType(TypeSymbol type)
        {
            return !(type == _symbolTable.Compilation.BoolType
                || type == _symbolTable.Compilation.CharType
                || type == _symbolTable.Compilation.StringType
                || type == _symbolTable.Compilation.IntType);
        }

        private void CheckNotLength(DesignatorNode node)
        {
            if (node is MemberAccessNode)
            {
                var memberAccessNode = (MemberAccessNode)node;
                if (memberAccessNode.Identifier == "length")
                {
                    if (_symbolTable.FindType(memberAccessNode.Designator) is ArrayTypeSymbol)
                    {
                        Error(node.Location, "Length must not be on the left side of an assignment");
                    }
                }
            }
        }

        public override void Visit(TypeCastNode node)
        {
            var sourceType = _symbolTable.FindType(node.Designator);
            var targetType = _symbolTable.FindType(node.Type) as ClassSymbol;
            if (targetType == null)
            {
                Error(node.Location, $"cannot cast to '{node.Type.Identifier}' as it is not a class", true);
            }
            _symbolTable.FixType(node, targetType);
        }

        private bool isAssignable(TypeSymbol sub, TypeSymbol baseType)
        {
            if (sub == null) { return false; }
            if (sub == _symbolTable.Compilation.NullType && IsReferenceType(baseType)) { return true; }

            if (sub is ArrayTypeSymbol && baseType is ArrayTypeSymbol)
            {
                return isAssignable(((ArrayTypeSymbol)sub).ElementType, ((ArrayTypeSymbol)baseType).ElementType);
            }

            if (sub is ClassSymbol && baseType is ClassSymbol)
            {
                var subClass = (ClassSymbol)sub;
                var baseClass = (ClassSymbol)baseType;
                if (subClass == baseClass)
                {
                    return true;
                }
                return isAssignable(subClass.BaseClass, baseClass);
            }

            return sub == baseType;
        }

        public override void Visit(ElementAccessNode node)
        {
            base.Visit(node);
            var expressionType = _symbolTable.FindType(node.Expression);

            if (expressionType != _symbolTable.Compilation.IntType)
            {
                Error(node.Location, $"Expression in element access must be of type int");
            }
        }

        public override void Visit(AssignmentNode node)
        {
            base.Visit(node);
            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);
            CheckIntegerMaxValue(node.Right);
            CheckNotLength(node.Left);
            if (!isAssignable(rightType, leftType))
            {
                Error(node.Location, $"Cannot assign '{rightType?.ToString()}' to '{leftType?.ToString()}'");
            }
        }

        public override void Visit(MethodCallNode node)
        {
            base.Visit(node);
            var methodSymbol = (MethodSymbol)_symbolTable.GetTarget(node.Designator);
            if (methodSymbol == null)
            {
                Error(node.Location, $"Undeclared method '{node.Designator}'", true);
            }
            var returnType = methodSymbol.ReturnType;
            if (methodSymbol.Parameters.Count != node.Arguments.Count)
            {
                Error(node.Location, $"Invalid argument count");
                return;
            }
            for (var i = 0; i < node.Arguments.Count; i += 1)
            {
                var param = methodSymbol.Parameters[i];
                var paramType = param.Type;
                var arg = node.Arguments[i];
                var argType = _symbolTable.FindType(arg);
                if (!isAssignable(argType, paramType))
                {
                    Error(arg.Location, $"Cannot assign argument of type {argType} to parameter {param.Identifier} ({paramType})");
                }
            }

            _symbolTable.FixType(node, returnType);
        }

        public override void Visit(CallStatementNode node)
        {
            var returnType = _symbolTable.FindType(node.Call.Designator);
            if (returnType != null)
            {
                Error(node.Location, $"Method as Statement must return void {returnType}");
            }
            base.Visit(node);
        }

        public override void Visit(ReturnStatementNode node)
        {
            base.Visit(node);

            if (node.Expression == null)
            {
                if (_method.ReturnType != null)
                {
                    Error(node.Location, $"Method return type {_method.ReturnType.Identifier} does not match return expression type null", true);
                }
                else
                {
                    return;
                }
            }

            if (_method.ReturnType == null)
            {
                if (node.Expression != null)
                {
                    Error(node.Location, $"Method return type void does not match return expression type {node.Expression}", true);
                }
            }

            var returnType = _symbolTable.FindType(node.Expression);

            if (!isAssignable(returnType, _method.ReturnType))
            {
                Error(node.Location, $"Method return type {_method.ReturnType.Identifier} does not match return expression type {returnType.Identifier}", true);
            }
        }

        private void checkBoolCondition(ExpressionNode condition)
        {
            var type = _symbolTable.FindType(condition);
            if (type != _symbolTable.Compilation.BoolType)
            {
                Error(condition.Location, $"Condition must be of type bool");
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
            if (type == null)
            {
                Error(node.Location, $"Undeclared type '{node.Type.Identifier}'", true);
            }
            _symbolTable.FixType(node, type);
        }

        public override void Visit(ArrayCreationNode node)
        {
            base.Visit(node);
            var arrayType = _symbolTable.FindType(new ArrayTypeNode(node.Location, node.ElementType));
            var expressionType = _symbolTable.FindType(node.Expression);

            if (expressionType != _symbolTable.Compilation.IntType)
            {
                Error(node.Location, "Invalid expression type in array creation");

            }

            _symbolTable.FixType(node, arrayType);

        }
    }
}
