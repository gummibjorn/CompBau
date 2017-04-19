namespace RappiSharp.VirtualMachine.Descriptors {
  internal sealed class InbuiltType : TypeDescriptor {
    private InbuiltType() { }

    public static InbuiltType Bool { get; } = new InbuiltType();
    public static InbuiltType Char { get; } = new InbuiltType();
    public static InbuiltType Int { get; } = new InbuiltType();
    public static InbuiltType String { get; } = new InbuiltType();
  }
}
