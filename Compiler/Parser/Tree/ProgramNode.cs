using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ProgramNode : Node {
    public List<ClassNode> Classes { get; }
    
    public ProgramNode(Location location, List<ClassNode> classes) :
      base(location) {
      if (classes == null) {
        throw new ArgumentNullException(nameof(classes));
      }
      Classes = classes;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = string.Empty;
      for (int index = 0; index < Classes.Count; index++) { 
        text += Classes[index];
        if (index < Classes.Count - 1) {
          text += "\r\n\r\n";
        }
      }
      return text;
    }
  }
}
