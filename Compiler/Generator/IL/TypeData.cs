using System.Xml.Serialization;

namespace RappiSharp.IL {
  [XmlType]
  [XmlInclude(typeof(ClassData))]
  [XmlInclude(typeof(ArrayData))]
  public class TypeData {
    public const int BoolType = -1;
    public const int CharType = -2;
    public const int IntType = -3;
    public const int StringType = -4;
  }
}
