using System;
using System.Collections.Generic;

namespace RappiSharp.IL {
  [Serializable]
  public class ClassData : TypeData {
    public string Identifier { get; set; }
    public int? BaseType { get; set; }
    public List<int> FieldTypes { get; } = new List<int>();
    public List<int> Methods { get; } = new List<int>();
  }
}
