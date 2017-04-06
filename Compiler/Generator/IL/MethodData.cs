using System.Collections.Generic;
using System.Xml.Serialization;

namespace RappiSharp.IL {
  [XmlType]
  public class MethodData {
    public string Identifier { get; set; }
    public int? ReturnType { get; set; }
    public List<int> ParameterTypes { get; } = new List<int>();
    public List<int> LocalTypes { get; } = new List<int>();
    public List<Instruction> Code { get; } = new List<Instruction>();

    public const int HaltMethod = -1;
    public const int WriteCharMethod = -2;
    public const int WriteIntMethod = -3;
    public const int WriteStringMethod = -4;
    public const int ReadCharMethod = -5;
    public const int ReadIntMethod = -6;
    public const int ReadStringMethod = -7;
  }
}
