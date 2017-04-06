using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RappiSharp.IL {
  [XmlType]
  public class Metadata {
    public List<TypeData> Types { get; } = new List<TypeData>();
    public List<MethodData> Methods { get; } = new List<MethodData>();
    public int MainMethod { get; set; }
    
    public static Metadata Load(string file) {
      var serializer = new XmlSerializer(typeof(Metadata));
      using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read)) {
        return (Metadata)serializer.Deserialize(stream);
      }
    }

    public void Save(string file) {
      var serializer = new XmlSerializer(typeof(Metadata));
      using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write)) {
        serializer.Serialize(stream, this);
      }
    }
  }
}
