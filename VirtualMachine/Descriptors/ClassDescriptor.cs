namespace RappiSharp.VirtualMachine.Descriptors {
  internal sealed class ClassDescriptor : TypeDescriptor {
    // TODO: Implement support for inheritance later
    public TypeDescriptor[] FieldTypes { get; set; }
  }
}
