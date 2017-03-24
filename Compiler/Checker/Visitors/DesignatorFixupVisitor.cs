using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System.Linq;

namespace RappiSharp.Compiler.Checker.Visitors
{
    internal class DesignatorFixupVisitor : Visitor
    {
        private readonly SymbolTable _symbolTable;
        private readonly MethodSymbol _method;
        private StatementNode _statement;

        public DesignatorFixupVisitor(SymbolTable symbolTable, MethodSymbol method)
        {
            _symbolTable = symbolTable;
            _method = method;
        }

        public override void Visit(BasicDesignatorNode node)
        {
            var symbol = _symbolTable.Find(_method, node.Identifier);
            if (symbol == null)
            {
                Error(node.Location, $"Designator {node.Identifier} is not defined");
            }
            else if (symbol is LocalVariableSymbol)
            {
                if (!((LocalVariableSymbol)symbol).VisibleIn.Contains(_statement))
                {
                    Error(node.Location, $"Local variable {symbol} is not visible");
                }
            }
            _symbolTable.FixTarget(node, symbol);
            _symbolTable.FixType(node, GetType(symbol));
        }

        public override void Visit(ElementAccessNode node)
        {
            base.Visit(node);
            var type = _symbolTable.FindType(node.Designator);
            if (type is ArrayTypeSymbol)
            {
                _symbolTable.FixType(node, ((ArrayTypeSymbol)type).ElementType);
            }
            else
            {
                Error(node.Location, $"Designator {node} does not refer to an array type");
                _symbolTable.FixType(node, null);
            }
        }

        public override void Visit(MemberAccessNode node)
        {
            base.Visit(node);
            var type = _symbolTable.FindType(node.Designator);
            Symbol target = null;
            TypeSymbol targetType = null;
            if (type is ClassSymbol)
            {
                var member = type.AllDeclarations.
                  Where(declaration => declaration.Identifier == node.Identifier).
                  SingleOrDefault();
                if (member == null)
                {
                    Error(node.Location, $"Designator {node} refers to an inexistent member {node.Identifier} in class {type}");
                }
                else
                {
                    target = member;
                    targetType = GetType(member);
                }
            }
            else if (type is ArrayTypeSymbol)
            {
                var arrayLength = _symbolTable.Compilation.ArrayLength;
                if (node.Identifier == arrayLength.Identifier)
                {
                    target = arrayLength;
                    targetType = GetType(arrayLength);
                }
                else
                {
                    Error(node.Location, $"Invalid member access {node.Identifier} on array {node}");
                }
            }
            else
            {
                Error(node.Location, $"Designator {node} does not refer to a class type");
            }
            _symbolTable.FixTarget(node, target);
            _symbolTable.FixType(node, targetType);
        }

        public override void Visit(AssignmentNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        public override void Visit(CallStatementNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        public override void Visit(IfStatementNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        public override void Visit(ReturnStatementNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        public override void Visit(WhileStatementNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        public override void Visit(LocalDeclarationNode node)
        {
            _statement = node;
            base.Visit(node);
        }

        private TypeSymbol GetType(Symbol symbol)
        {
            if (symbol is VariableSymbol)
            {
                return ((VariableSymbol)symbol).Type;
            }
            else if (symbol is ConstantSymbol)
            {
                return ((ConstantSymbol)symbol).Type;
            }
            else if (symbol is MethodSymbol)
            {
                return ((MethodSymbol)symbol).ReturnType;
            }
            else
            {
                return null;
            }
        }

        private void Error(Location location, string message)
        {
            Diagnosis.ReportError(location, $"{message}");
        }
    }
}
