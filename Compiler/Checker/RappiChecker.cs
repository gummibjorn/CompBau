using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Phases;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.Compiler.Checker.Visitors;
using System.Linq;

namespace RappiSharp.Compiler.Checker {
  internal class RappiChecker {
    private readonly ProgramNode _syntaxTree;

    public SymbolTable SymbolTable { get; } = new SymbolTable();
    
    public RappiChecker(ProgramNode syntaxTree) {
      _syntaxTree = syntaxTree;
      SymbolConstruction.Run(SymbolTable, _syntaxTree);
      TypeResolution.Run(SymbolTable);
      FixSyntaxTree();
      CheckSingleMain();
    }

    private void FixSyntaxTree() {
      foreach (var classSymbol in SymbolTable.Compilation.Classes) {
        foreach (var method in classSymbol.Methods) {
          var methodNode = SymbolTable.GetDeclarationNode<MethodNode>(method);
          var body = methodNode.Body;
          body.Accept(new DesignatorFixupVisitor(SymbolTable, method));
          body.Accept(new TypeCheckVisitor(SymbolTable, method));
        }
      }
    }

    private void CheckSingleMain() {
      var entries =
        from classSymbol in SymbolTable.Compilation.Classes
        from method in classSymbol.Methods
        where method.Identifier == "Main"
        select method;
      if (entries.Count() == 0) {
        Diagnosis.ReportError("Main method missing");
      } else if (entries.Count() > 1) {
        Diagnosis.ReportError("Only one Main() method is permitted");
      } else {
        var main = entries.Single();
        SymbolTable.Compilation.MainMethod = main;
        if (main.Parameters.Count > 0) {
          Diagnosis.ReportError("Main method must have no parameters");
        }
        if (main.ReturnType != null) {
          Diagnosis.ReportError("Main method must be void");
        }
      }
    }
  }
}
