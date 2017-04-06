using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.IL;
using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Generator.Emit {
  internal class ILBuilder {
    private readonly SymbolTable _symbolTable;
    private readonly Dictionary<string, int> _typeRefs = new Dictionary<string, int>();
    private readonly Dictionary<MethodSymbol, int> _methodRefs = new Dictionary<MethodSymbol, int>();

    public Metadata Metadata { get; } = new Metadata();

    public ILBuilder(SymbolTable symbolTable) {
      _symbolTable = symbolTable;
      GenerateMetadata();
    }

    public MethodData GetMethod(MethodSymbol method) {
      return Metadata.Methods[GetMethodRef(method)];
    }

    public void Complete() {
      foreach (var methodData in Metadata.Methods) {
        FixOperands(methodData.Code);
      }
    }

    private void FixOperands(List<Instruction> code) {
      foreach (var instruction in code) {
        if (instruction.Operand is TypeSymbol) {
          instruction.Operand = GetTypeRef((TypeSymbol)instruction.Operand);
        } else if (instruction.Operand is MethodSymbol) {
          instruction.Operand = GetMethodRef((MethodSymbol)instruction.Operand);
        }
      }
    }

    private int GetTypeRef(TypeSymbol type) {
      if (type is BaseTypeSymbol) {
        var scope = _symbolTable.Compilation;
        if (type == scope.BoolType) {
          return TypeData.BoolType;
        } else if (type == scope.CharType) {
          return TypeData.CharType;
        } else if (type == scope.IntType) {
          return TypeData.IntType;
        } else if (type == scope.StringType) {
          return TypeData.StringType;
        }
        throw new NotSupportedException();
      } else {
        return _typeRefs[type.Identifier];
      }
    }

    private int GetMethodRef(MethodSymbol method) {
      return _methodRefs[method];
    }

    private void GenerateMetadata() {
      foreach (var type in _symbolTable.Compilation.Types) {
        RegisterType(type);
      }
      foreach (var type in _symbolTable.Compilation.Types) {
        FixType(type);
      }
      FixMainMethod();
    }

    private void RegisterType(TypeSymbol type) {
      if (type is ClassSymbol) {
        var classData = new ClassData() {
          Identifier = type.Identifier
        };
        Metadata.Types.Add(classData);
        _typeRefs.Add(type.Identifier, Metadata.Types.IndexOf(classData));
        foreach (var method in ((ClassSymbol)type).Methods) {
          RegisterMethod(method);
        }
      } else if (type is ArrayTypeSymbol) {
        var arrayData = new ArrayData();
        Metadata.Types.Add(arrayData);
        _typeRefs.Add(type.Identifier, Metadata.Types.IndexOf(arrayData));
      }
    }

    private void RegisterMethod(MethodSymbol method) {
      var methodData = new MethodData() {
        Identifier = method.Identifier
      };
      Metadata.Methods.Add(methodData);
      _methodRefs.Add(method, Metadata.Methods.IndexOf(methodData));
    }

    private void FixType(TypeSymbol type) {
      if (type is ClassSymbol) {
        FixClassType((ClassSymbol)type);
      } else if (type is ArrayTypeSymbol) {
        FixArrayType((ArrayTypeSymbol)type);
      }
    }

    private void FixArrayType(ArrayTypeSymbol arrayType) {
      var arrayData = (ArrayData)Metadata.Types[GetTypeRef(arrayType)];
      arrayData.ElementType = GetTypeRef(arrayType.ElementType);
    }

    private void FixClassType(ClassSymbol classSymbol) {
      var classData = (ClassData)Metadata.Types[GetTypeRef(classSymbol)];
      if (classSymbol.BaseClass != null) {
        classData.BaseType = GetTypeRef(classSymbol.BaseClass);
      }
      foreach (var field in classSymbol.Fields) {
        classData.FieldTypes.Add(GetTypeRef(field.Type));
      }
      foreach (var method in classSymbol.Methods) {
        classData.Methods.Add(GetMethodRef(method));
        FixMethod(method);
      }
    }

    private void FixMethod(MethodSymbol method) {
      var methodData = Metadata.Methods[GetMethodRef(method)];
      if (method.ReturnType != null) {
        methodData.ReturnType = GetTypeRef(method.ReturnType);
      }
      foreach (var parameter in method.Parameters) {
        methodData.ParameterTypes.Add(GetTypeRef(parameter.Type));
      }
      foreach (var local in method.LocalVariables) {
        methodData.LocalTypes.Add(GetTypeRef(local.Type));
      }
    }

    private void FixMainMethod() {
      var mainMethod = _symbolTable.Compilation.MainMethod;
      var mainType = (ClassSymbol)mainMethod.Scope;
      Metadata.MainMethod = GetMethodRef(mainMethod);
    }
  }
}
