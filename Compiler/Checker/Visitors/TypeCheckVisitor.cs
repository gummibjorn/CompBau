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

        // TODO: Implement type resolution for expressions (except designator)
        // TODO: Implement type checks for entire program
        public override void Visit(BinaryExpressionNode node)
        {
            base.Visit(node);
            var leftType = _symbolTable.FindType(node.Left);
            var rightType = _symbolTable.FindType(node.Right);

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
                        throw new System.Exception("Wrong type in binary expression");
                    }
                    break;
            }
        }
    }
}
