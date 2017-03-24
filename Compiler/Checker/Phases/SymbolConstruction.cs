using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Checker.Visitors;
using RappiSharp.Compiler.Parser.Tree;
using System.Linq;

namespace RappiSharp.Compiler.Checker.Phases {
  internal class SymbolConstruction {
    private readonly SymbolTable _symbolTable;

    public static void Run(SymbolTable symbolTable, ProgramNode programNode) {
      var construction = new SymbolConstruction(symbolTable);
      construction.RegisterDeclarations(programNode);
      construction.CheckValidIdentifiers();
      construction.CheckUniqueIdentifiers();
    }

    private SymbolConstruction(SymbolTable symbolTable) {
      _symbolTable = symbolTable;
    }

    private void RegisterDeclarations(ProgramNode programNode) {
      RegisterBuiltIns();
      foreach (var classNode in programNode.Classes) {
        RegisterClass(classNode);
      }
    }

    private void RegisterBuiltIns() {
      RegisterBuiltInTypes();
      RegisterBuiltInMethods();
      RegisterBuiltInConstants();
      RegisterBuiltInField();
    }

    private void RegisterBuiltInField() {
      var arrayLength = new FieldSymbol(GlobalScope, "length");
      arrayLength.Type = _symbolTable.FindType("int");
      GlobalScope.ArrayLength = arrayLength;
    }

    private void RegisterBuiltInConstants() {
      GlobalScope.NullConstant = RegisterBuiltInConstant(GlobalScope.NullType, "null");
      GlobalScope.FalseConstant = RegisterBuiltInConstant(_symbolTable.FindType("bool"), "false");
      GlobalScope.TrueConstant = RegisterBuiltInConstant(_symbolTable.FindType("bool"), "true");
    }

    private ConstantSymbol RegisterBuiltInConstant(TypeSymbol type, string name) {
      var constant = new ConstantSymbol(GlobalScope, name, type);
      GlobalScope.Constants.Add(constant);
      return constant;
    }

    private void RegisterBuiltInTypes() {
      GlobalScope.BoolType = RegisterBuiltInType("bool");
      GlobalScope.CharType = RegisterBuiltInType("char");
      GlobalScope.IntType = RegisterBuiltInType("int");
      GlobalScope.StringType = RegisterBuiltInType("string");
    }

    private BaseTypeSymbol RegisterBuiltInType(string name) {
      var type = new BaseTypeSymbol(GlobalScope, name);
      GlobalScope.Types.Add(type);
      return type;
    }

    private void RegisterBuiltInMethods() {
      RegisterBuiltInMethod(null, "Halt", _symbolTable.FindType("string"));
      RegisterBuiltInMethod(null, "WriteInt", _symbolTable.FindType("int"));
      RegisterBuiltInMethod(null, "WriteChar", _symbolTable.FindType("char"));
      RegisterBuiltInMethod(null, "WriteString", _symbolTable.FindType("string"));
      RegisterBuiltInMethod(_symbolTable.FindType("int"), "ReadInt", null);
      RegisterBuiltInMethod(_symbolTable.FindType("char"), "ReadChar", null);
      RegisterBuiltInMethod(_symbolTable.FindType("string"), "ReadString", null);
    }

    private void RegisterBuiltInMethod(TypeSymbol returnType, string identifier, TypeSymbol parameterType) {
      var method = new MethodSymbol(GlobalScope, identifier);
      GlobalScope.Methods.Add(method);
      method.ReturnType = returnType;
      if (parameterType != null) {
        var parameter = new ParameterSymbol(method, "value");
        parameter.Type = parameterType;
        method.Parameters.Add(parameter);
      }
    }

    private void RegisterClass(ClassNode classNode) {
      var classSymbol = new ClassSymbol(GlobalScope, classNode.Identifier);
      GlobalScope.Classes.Add(classSymbol);
      GlobalScope.Types.Add(classSymbol);
      _symbolTable.LinkDeclaration(classNode, classSymbol);
      foreach (var variableNode in classNode.Variables) {
        RegisterField(classSymbol, variableNode);
      }
      foreach (var methodNode in classNode.Methods) {
        RegisterMethod(classSymbol, methodNode);
      }
    }

    private void RegisterField(ClassSymbol classSymbol, VariableNode variableNode) {
      var fieldSymbol = new FieldSymbol(classSymbol, variableNode.Identifier);
      classSymbol.Fields.Add(fieldSymbol);
      _symbolTable.LinkDeclaration(variableNode, fieldSymbol);
    }

    private void RegisterMethod(ClassSymbol classSymbol, MethodNode methodNode) {
      var methodSymbol = new MethodSymbol(classSymbol, methodNode.Identifier);
      classSymbol.Methods.Add(methodSymbol);
      _symbolTable.LinkDeclaration(methodNode, methodSymbol);
      foreach (var parameterNode in methodNode.Parameters) {
        RegisterParameter(methodSymbol, parameterNode);
      }
      methodNode.Body.Accept(new LocalVariableVisitor(_symbolTable, methodSymbol));
    }

    private void RegisterParameter(MethodSymbol methodSymbol, VariableNode parameterNode) {
      var parameterSymbol = new ParameterSymbol(methodSymbol, parameterNode.Identifier);
      methodSymbol.Parameters.Add(parameterSymbol);
      _symbolTable.LinkDeclaration(parameterNode, parameterSymbol);
    }

    private void CheckValidIdentifiers() {
      foreach (var classSymbol in GlobalScope.Classes) {
        CheckValidIdentifier(classSymbol);
        foreach (var field in classSymbol.Fields) {
          CheckValidIdentifier(field);
        }
        foreach (var method in classSymbol.Methods) {
          CheckValidIdentifier(method);
          foreach (var member in method.AllDeclarations) {
            CheckValidIdentifier(member);
          }
        }
      }
    }

    private readonly string[] _reserved =
      { "bool", "char", "false", "int", "string", "this", "true", "null", "void" };

    private void CheckValidIdentifier(Symbol symbol) {
      if (_reserved.Contains(symbol.Identifier)) {
        Error(symbol, $"Reserved keyword {symbol} used as an identifier");
      }
    }

    private void CheckUniqueIdentifiers() {
      CheckUniqueIdentifiers(GlobalScope);
      foreach (var classSymbol in GlobalScope.Classes) {
        CheckUniqueIdentifiers(classSymbol);
        foreach (var method in classSymbol.Methods) {
          CheckUniqueIdentifiers(method);
        }
      }
    }

    private void CheckUniqueIdentifiers(Symbol scope) {
      foreach (var declaration in scope.AllDeclarations) {
        if (scope.AllDeclarations.Where(symbol => symbol.Identifier == declaration.Identifier).Count() > 1) {
          Error(declaration, $"Identifier {declaration} is declared more than once in the scope");
        }
      }
    }

    private CompilationUnit GlobalScope {
      get {
        return _symbolTable.Compilation;
      }
    }

    private void Error(Symbol symbol, string message) {
      var node = _symbolTable.GetDeclarationNode<Node>(symbol);
      var location = node == null ? new Location() : node.Location;
      Error(location, message);
    }

    private void Error(Location location, string message) {
      Diagnosis.ReportError($"{message} LOCATION {location}");
    }
  }
}
