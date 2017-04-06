using System;

namespace RappiSharp.IL {
  [Serializable]
  public class Instruction {
    public OpCode OpCode { get; set; }
    public object Operand { get; set; }

        public override string ToString()
        {
            return $"{OpCode.ToString()} {Operand?.ToString()}";
        }
    }

}
