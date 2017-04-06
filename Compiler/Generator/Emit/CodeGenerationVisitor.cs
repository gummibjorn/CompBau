using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;

namespace RappiSharp.Compiler.Generator.Emit {
  internal class CodeGenerationVisitor : Visitor {
    private readonly SymbolTable _symbolTable;
    private readonly MethodSymbol _method;
    private readonly ILAssembler _assembler;

        public CodeGenerationVisitor(SymbolTable symbolTable, MethodSymbol method, ILAssembler assembler) {
          _symbolTable = symbolTable;
          _method = method;
          _assembler = assembler;
        }

        public override void Visit(ArrayCreationNode node)
        {
            base.Visit(node);

            var type = _symbolTable.FindType(node);

            _assembler.Emit(OpCode.newarr, type);
        }

        public override void Visit(IntegerLiteralNode node)
        {
            base.Visit(node);

            _assembler.Emit(OpCode.ldc_i, (int)node.Value);
        }


        public override void Visit(AssignmentNode node)
        {
            base.Visit(node);
            var target = _symbolTable.GetTarget(node.Left);
            if(target is ParameterSymbol)
            {
                var index = _method.Parameters.IndexOf((ParameterSymbol)target);
                _assembler.Emit(OpCode.starg, index);
            } else if (target is LocalVariableSymbol)
            {
                var index = _method.LocalVariables.IndexOf((LocalVariableSymbol)target);
                _assembler.Emit(OpCode.stloc, index);
            } else
            {
                _assembler.Emit(OpCode.stfld, target);
            }
            //TODO how do i figure out whether it's an array assignment?

        }
    }
}
