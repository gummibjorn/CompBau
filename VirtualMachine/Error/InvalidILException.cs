using System;

namespace RappiSharp.VirtualMachine.Error {
  [Serializable]
  internal class InvalidILException : VMException {
    public InvalidILException(string message) :
      base($"Invalid IL code: {message}") {
    }
  }
}
