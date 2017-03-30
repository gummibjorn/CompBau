using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;

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
                    if(type == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, $"Invalid type {type.ToString()} for unary '!'");
                    }
                    break;
                case Operator.Plus:
                    checkIntegerMaxValue(operand);
                    if(type == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    } else
                    {
                        Diagnosis.ReportError(node.Location, $"Invalid type {type.ToString()} for unary {optr.ToString()}");
                    }
                    break;
                case Operator.Minus:
                    if(type == _symbolTable.Compilation.IntType)
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
            if(node is IntegerLiteralNode)
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
                    if(leftType == _symbolTable.Compilation.BoolType && rightType == _symbolTable.Compilation.BoolType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }else
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
                    if(leftType == _symbolTable.Compilation.IntType && rightType == _symbolTable.Compilation.IntType)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.IntType);
                    }else
                    {
                        Diagnosis.ReportError(node.Location, "Invalid types in binary expression");
                        throw new System.Exception($"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}");
                    }
                    break;
                case Operator.Less:
                case Operator.LessEqual:
                case Operator.Greater:
                case Operator.GreaterEqual:
                case Operator.Equals:
                case Operator.Unequal:
                    if(leftType.Identifier == rightType.Identifier)
                    {
                        _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    }else
                    {
                        Diagnosis.ReportError(node.Location, "Invalid types in binary expression");
                        throw new System.Exception($"Wrong type in binary expression {leftType.ToString()} {node.Operator} {rightType.ToString()}");
                    }
                    break;
                case Operator.Is:
                    _symbolTable.FixType(node, _symbolTable.Compilation.BoolType);
                    break;
            }
        }

        // TODO: Implement type checks for entire program
        public override void Visit(AssignmentNode node)
        {
            base.Visit(node);
            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);
            checkIntegerMaxValue(node.Right);
            if(leftType != rightType)
            {
                Diagnosis.ReportError(node.Location, $"Cannot assign {rightType.ToString()} to {leftType.ToString()}");
            }
        }

        public override void Visit(MethodCallNode node)
        {
            base.Visit(node);
            var methodSymbol = _symbolTable.GetTarget(node.Designator);
            var methodDefinition = _symbolTable.GetDeclarationNode<MethodNode>(methodSymbol);
            if(methodDefinition.Parameters.Count != node.Arguments.Count)
            {
                Diagnosis.ReportError(node.Location, $"Invalid argument count");
                return;
            }
            for(var i = 0; i<node.Arguments.Count; i += 1)
            {
                var param = methodDefinition.Parameters[i];
                var paramType = _symbolTable.FindType(param.Type);
                var arg = node.Arguments[i];
                var argType = _symbolTable.FindType(arg);
                if(paramType != argType)
                {
                    Diagnosis.ReportError(arg.Location, $"Cannot assign argument of type {argType} to parameter {param.Identifier} ({paramType})");
                }
            }
        }
    }
}
