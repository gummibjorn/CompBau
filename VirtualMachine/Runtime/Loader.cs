using RappiSharp.IL;
using RappiSharp.VirtualMachine.Descriptors;
using RappiSharp.VirtualMachine.Error;
using System;
using System.Collections.Generic;

namespace RappiSharp.VirtualMachine.Runtime {
  internal sealed class Loader {
    private readonly Metadata _metadata;
    private readonly Dictionary<int, TypeDescriptor> _types = new Dictionary<int, TypeDescriptor>();
    private readonly Dictionary<int, MethodDescriptor> _methods = new Dictionary<int, MethodDescriptor>();

    public MethodDescriptor MainMethod { get; }

    public Loader(Metadata metadata) {
      _metadata = metadata;
      RegisterInbuilts();
      try {
        RegisterTypes();
        RegisterMethods();
        FixTypes();
        FixMethods();
        MainMethod = _methods[_metadata.MainMethod];
      } catch (Exception) {
        throw new InvalidILException($"Invalid IL format");
      }
    }

    private void FixMethods() {
      for (var index = 0; index < _metadata.Methods.Count; index++) {
        var methodDescriptor = _methods[index];
        var methodData = _metadata.Methods[index];
        FixMethod(methodDescriptor, methodData);
      }
    }

    private void FixMethod(MethodDescriptor methodDescriptor, MethodData methodData) {
      if (methodData.ReturnType != null) {
        methodDescriptor.ReturnType = _types[methodData.ReturnType.Value];
      }
      methodDescriptor.ParameterTypes = MapTypes(methodData.ParameterTypes);
      methodDescriptor.LocalTypes = MapTypes(methodData.LocalTypes);
      methodDescriptor.Code = methodData.Code;
      FixCode(methodDescriptor.Code);
    }

    private void FixCode(List<Instruction> code) {
      foreach (var instruction in code) {
        switch (instruction.OpCode) {
          case OpCode.isinst:
          case OpCode.castclass:
          case OpCode.newobj:
            instruction.Operand = (ClassDescriptor)_types[(int)instruction.Operand];
            break;
          case OpCode.newarr:
            instruction.Operand = (ArrayDescriptor)_types[(int)instruction.Operand];
            break;
          case OpCode.call:
          case OpCode.callvirt:
            instruction.Operand = _methods[(int)instruction.Operand];
            break;
        }
      }
    }

    private TypeDescriptor[] MapTypes(List<int> typeIndices) {
      var result = new TypeDescriptor[typeIndices.Count];
      for (var index = 0; index < typeIndices.Count; index++) {
        result[index] = _types[typeIndices[index]];
      }
      return result;
    }

    private void FixTypes() {
      for (int index = 0; index < _metadata.Types.Count; index++) {
        var typeDescriptor = _types[index];
        var typeData = _metadata.Types[index];
        FixType(typeDescriptor, typeData);
      }
    }

    private void FixType(TypeDescriptor typeDescriptor, TypeData typeData) {
      if (typeDescriptor is ArrayDescriptor) {
        FixArrayType((ArrayDescriptor)typeDescriptor, (ArrayData)typeData);
      } else {
        FixClassType((ClassDescriptor)typeDescriptor, (ClassData)typeData);
      }
    }

    private void FixClassType(ClassDescriptor classDescriptor, ClassData classData) {
      // TODO: Implement support for inheritance later
      classDescriptor.FieldTypes = MapTypes(classData.FieldTypes);
      FixMethodParent(classDescriptor, classData);
    }

    private void FixMethodParent(ClassDescriptor classDescriptor, ClassData classData) {
      foreach (var methodIndex in classData.Methods) {
        var methodDescriptor = _methods[methodIndex];
        methodDescriptor.Parent = classDescriptor;
      }
    }

    private void FixArrayType(ArrayDescriptor arrayDescriptor, ArrayData arrayData) {
      arrayDescriptor.ElementType = _types[arrayData.ElementType];
    }

    private void RegisterMethods() {
      for (var index = 0; index < _metadata.Methods.Count; index++) {
        var methodData = _metadata.Methods[index];
        _methods.Add(index, new MethodDescriptor(methodData.Identifier));
      }
    }

    private void RegisterTypes() {
      for (var index = 0; index < _metadata.Types.Count; index++) {
        _types.Add(index, CreateTypeDescriptor(_metadata.Types[index]));
      }
    }

    private TypeDescriptor CreateTypeDescriptor(TypeData typeData) {
      if (typeData is ArrayData) {
        return new ArrayDescriptor();
      } else if (typeData is ClassData) {
        return new ClassDescriptor();
      } else {
        throw new InvalidILException("Invalid type descriptor");
      }
    }

    private void RegisterInbuilts() {
      _types.Add(TypeData.BoolType, InbuiltType.Bool);
      _types.Add(TypeData.CharType, InbuiltType.Char);
      _types.Add(TypeData.IntType, InbuiltType.Int);
      _types.Add(TypeData.StringType, InbuiltType.String);
      _methods.Add(MethodData.HaltMethod, MethodDescriptor.Halt);
      _methods.Add(MethodData.WriteCharMethod, MethodDescriptor.WriteChar);
      _methods.Add(MethodData.WriteIntMethod, MethodDescriptor.WriteInt);
      _methods.Add(MethodData.WriteStringMethod, MethodDescriptor.WriteString);
      _methods.Add(MethodData.ReadCharMethod, MethodDescriptor.ReadChar);
      _methods.Add(MethodData.ReadIntMethod, MethodDescriptor.ReadInt);
      _methods.Add(MethodData.ReadStringMethod, MethodDescriptor.ReadString);
    }
  }
}
