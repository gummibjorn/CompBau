using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Checker.Visitors {
  internal class LocalVariableVisitor : Visitor {
    private readonly SymbolTable _symbolTable;
    private readonly MethodSymbol _method;
    private Stack<HashSet<LocalVariableSymbol>> _stack = new Stack<HashSet<LocalVariableSymbol>>();

    public LocalVariableVisitor(SymbolTable symbolTable, MethodSymbol method) {
      _symbolTable = symbolTable;
      _method = method;
    }

    public override void Visit(StatementBlockNode node) {
      _stack.Push(new HashSet<LocalVariableSymbol>());
      base.Visit(node);
      _stack.Pop();
    }

    public override void Visit(LocalDeclarationNode node) {
      var variable = node.Variable;
      var localSymbol = new LocalVariableSymbol(_method, variable.Identifier);
      _method.LocalVariables.Add(localSymbol);
      _symbolTable.LinkDeclaration(variable, localSymbol);
      _stack.Peek().Add(localSymbol);
    }

    public override void Visit(AssignmentNode node) {
      Record(node);
    }

    public override void Visit(IfStatementNode node) {
      Record(node);
      base.Visit(node);
    }

    public override void Visit(WhileStatementNode node) {
      Record(node);
      base.Visit(node);
    }

    public override void Visit(CallStatementNode node) {
      Record(node);
    }

    public override void Visit(ReturnStatementNode node) {
      Record(node);
    }

    private void Record(StatementNode statement) {
      foreach (var scope in _stack) {
        foreach (var localSymbol in scope) {
          localSymbol.VisibleIn.Add(statement);
        }
      }
    }
  }
}
