namespace RappiSharp.VirtualMachine.Descriptors {
  internal sealed class ArrayDescriptor : TypeDescriptor {
    public TypeDescriptor ElementType { get; set; }
  }
}
