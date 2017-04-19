using RappiSharp.VirtualMachine.Error;
using System.Collections.Generic;

namespace RappiSharp.VirtualMachine.Runtime {
  internal class CallStack {
    private const int Limit = 100000;

    private readonly Stack<ActivationFrame> _stack = new Stack<ActivationFrame>();

    public int Count {
      get {
        return _stack.Count;
      }
    }

    public void Push(ActivationFrame frame) {
      if (_stack.Count == Limit) {
        throw new VMException("Stack overflow");
      }
      _stack.Push(frame);
    }

    public ActivationFrame Peek() {
      return _stack.Peek();
    }

    public void Pop() {
      _stack.Pop();
    }

    public IEnumerator<ActivationFrame> GetEnumerator() {
      return _stack.GetEnumerator();
    }
  }
}
