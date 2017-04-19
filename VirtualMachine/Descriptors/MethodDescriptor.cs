using RappiSharp.IL;
using System;
using System.Collections.Generic;

namespace RappiSharp.VirtualMachine.Descriptors {
  internal sealed class MethodDescriptor {
    public string Identifier { get; }
    public ClassDescriptor Parent { get; set; }

    public TypeDescriptor ReturnType { get; set; }
    public TypeDescriptor[] ParameterTypes { get; set; } 
    public TypeDescriptor[] LocalTypes { get; set; }
    public List<Instruction> Code { get; set; }

    public MethodDescriptor(string identifier) {
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
    }

    public static MethodDescriptor Halt { get; } = new MethodDescriptor("Halt");
    public static MethodDescriptor WriteChar { get; } = new MethodDescriptor("WriteChar");
    public static MethodDescriptor WriteInt { get; } = new MethodDescriptor("WriteInt");
    public static MethodDescriptor WriteString { get; } = new MethodDescriptor("WriteString");
    public static MethodDescriptor ReadChar { get; } = new MethodDescriptor("ReadChar");
    public static MethodDescriptor ReadInt { get; } = new MethodDescriptor("ReadInt");
    public static MethodDescriptor ReadString { get; } = new MethodDescriptor("ReadString");
  }
}
