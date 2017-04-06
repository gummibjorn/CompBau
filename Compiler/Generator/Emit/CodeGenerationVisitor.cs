using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;

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

        // TODO: Implement IL code generation in this visitor
        public override void Visit(ArrayCreationNode node)
        {
            base.Visit(node);
        }
    }
}
