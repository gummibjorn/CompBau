using RappiSharp.VirtualMachine.Error;
using System.Collections.Generic;

namespace RappiSharp.VirtualMachine.Runtime {
  internal class EvaluationStack {
    private const int Limit = 100;
    private readonly Stack<object> _stack = new Stack<object>();

    public int Count {
      get {
        return _stack.Count;
      }
    }

    public void Push(object value) {
      if (_stack.Count == Limit) {
        throw new InvalidILException("Evaluation stack limit exceeded");
      }
      _stack.Push(value);
    }

    public object Pop() {
      if (_stack.Count == 0) {
        throw new InvalidILException($"Evaluation stack underflow");
      }
      return _stack.Pop();
    }

    public T Pop<T>() {
      var value = Pop();
      if (value == null) {
            throw new InvalidILException($"null deref!");
      }
      if (!(value is T)) {
            throw new InvalidILException($"Expected {typeof(T)} instead of {value?.GetType()} ({value})");
      }
      return (T)value;
    }

    public object Peek() {
      if (_stack.Count == 0) {
        throw new InvalidILException($"Evaluation stack underflow");
      }
      return _stack.Peek();
    }
  }
}
