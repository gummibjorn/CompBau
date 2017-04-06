using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Generator.Emit;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;
using System.Linq;

namespace RappiSharp.Compiler.Generator {
  internal class RappiGenerator {
    private readonly SymbolTable _symbolTable;
    private readonly ILBuilder _ilBuilder;
    
    public RappiGenerator(SymbolTable symbolTable) {
      _symbolTable = symbolTable;
      _ilBuilder = new ILBuilder(symbolTable);
      GenerateIL();
    }

    public Metadata Metadata {
      get {
        return _ilBuilder.Metadata;
      }
    }
    
    private void GenerateIL() {
      var allMethods =
        from classSymbol in _symbolTable.Compilation.Classes
        from method in classSymbol.Methods
        select method;
      foreach (var method in allMethods) {
        GenerateIL(method);
      }
      _ilBuilder.Complete();
    }

    private void GenerateIL(MethodSymbol method) {
      var methodData = _ilBuilder.GetMethod(method);
      var methodNode = _symbolTable.GetDeclarationNode<MethodNode>(method);
      var assembler = new ILAssembler(methodData.Code);
      methodNode.Body.Accept(new CodeGenerationVisitor(_symbolTable, method, assembler));
      assembler.Complete();
    }
  }
}
