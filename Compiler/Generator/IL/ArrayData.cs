using System;

namespace RappiSharp.IL {
  [Serializable]
  public class ArrayData : TypeData {
    public int ElementType { get; set; }
  }
}
