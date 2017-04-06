using RappiSharp.IL;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Generator.Emit {
  internal class ILAssembler {
    private readonly List<Instruction> _code;

    public sealed class Label {
    }

    private readonly Dictionary<Label, int> _targets = new Dictionary<Label, int>();

    public ILAssembler(List<Instruction> code) {
      _code = code;
    }

    public void Complete() {
      if (_code.Count == 0 || _code[_code.Count - 1].OpCode != OpCode.ret) {
        Emit(OpCode.ret);
      }
      FixLabels();
    }

    public Label CreateLabel() {
      return new Label();
    }

    public void SetLabel(Label label) {
      _targets.Add(label, _code.Count);
    }

    private void FixLabels() {
      for (int source = 0; source < _code.Count; source++) {
        var operand = _code[source].Operand;
        if (operand is Label) {
          var target = _targets[(Label)operand];
          int delta = target - source - 1;
          _code[source].Operand = delta;
        }
      }
    }

    public void Emit(OpCode opCode) {
      Emit(opCode, null);
    }

    public void Emit(OpCode opCode, object operand) {
      _code.Add(new Instruction {
        OpCode = opCode,
        Operand = operand
      });
    }
  }
}
