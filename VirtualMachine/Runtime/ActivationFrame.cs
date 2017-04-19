using RappiSharp.VirtualMachine.Descriptors;

namespace RappiSharp.VirtualMachine.Runtime {
  internal class ActivationFrame {
    public MethodDescriptor Method { get; }
    public object ThisReference { get; }
    public EvaluationStack EvaluationStack { get; } = new EvaluationStack();
    public int InstructionPointer { get; set; }
    public object[] Arguments { get; }
    public object[] Locals { get; }

    public ActivationFrame(MethodDescriptor method, object thisReference, object[] arguments, object[] locals) {
      Method = method;
      ThisReference = thisReference;
      Arguments = arguments;
      Locals = locals;
    }
  }
}
