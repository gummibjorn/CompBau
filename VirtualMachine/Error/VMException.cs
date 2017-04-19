using System;

namespace RappiSharp.VirtualMachine.Error {
  [Serializable]
  internal class VMException : Exception {
    public VMException(string message) :
      base(message) {
    }
  }
}
