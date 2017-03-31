using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System.Collections.Generic;
using System.Linq;

namespace RappiSharp.Compiler.Checker.General {
  internal class SymbolTable {
    public CompilationUnit Compilation { get; } = new CompilationUnit();

    private readonly Dictionary<Symbol, Node> _symbolToNode = new Dictionary<Symbol, Node>();
    private readonly Dictionary<DesignatorNode, Symbol> _targetFixup = new Dictionary<DesignatorNode, Symbol>();
    private readonly Dictionary<ExpressionNode, TypeSymbol> _typeFixup = new Dictionary<ExpressionNode, TypeSymbol>();
    
    public TypeSymbol FindType(string identifier) {
      return
        (from type in Compilation.Types
         where type.Identifier == identifier
         select type).SingleOrDefault();
    }

    public TypeSymbol FindType(TypeNode node) {
      TypeSymbol result;
      if (node is BasicTypeNode) {
        var identifier = ((BasicTypeNode)node).Identifier;
        result = FindType(identifier);
      } else {
        var elementTypeNode = ((ArrayTypeNode)node).ElementType;
        var elementType = FindType(elementTypeNode);
        var identifier = elementType.Identifier + "[]";
        result = FindType(identifier);
        if (result == null) {
          result = new ArrayTypeSymbol(Compilation, elementType);
          Compilation.Types.Add(result);
        }
      }
      return result;
    }

    public Symbol Find(Symbol scope, string identifier) {
      if (scope == null) {
        return null;
      }
      foreach (Symbol declaration in scope.AllDeclarations) {
        if (declaration.Identifier == identifier) {
          return declaration;
        }
      }
      return Find(scope.Scope, identifier);
    }

    public void LinkDeclaration(Node node, Symbol symbol) {
      _symbolToNode.Add(symbol, node);
    }

    public T GetDeclarationNode<T>(Symbol symbol) where T: Node {
      if (_symbolToNode.ContainsKey(symbol)) {
        return (T)_symbolToNode[symbol];
      } else {
        return null;
      }
    }

    public void FixTarget(DesignatorNode node, Symbol symbol) {
      _targetFixup.Add(node, symbol);
    }

    public Symbol GetTarget(DesignatorNode node) {
      return _targetFixup[node];
    }

    public void FixType(ExpressionNode node, TypeSymbol symbol) {
      _typeFixup.Add(node, symbol);
    }

    public TypeSymbol FindType(ExpressionNode node) {
            try
            {
              return _typeFixup[node];

            } catch(KeyNotFoundException e)
            {
                //Diagnosis.ReportError(node.Location, "Type not found for " + node.ToString());
                throw e;
            }
    }
  }
}
