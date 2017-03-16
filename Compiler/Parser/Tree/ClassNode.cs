using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class ClassNode : Node {
    public string Identifier { get; }
    public BasicTypeNode BaseClass { get; } // null if none
    public List<VariableNode> Variables { get; }
    public List<MethodNode> Methods { get; }
    
    public ClassNode(Location location, string identifier, BasicTypeNode baseClass, 
        List<VariableNode> variables, List<MethodNode> methods) :
      base(location) {
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
      BaseClass = baseClass;
      if (variables == null) {
        throw new ArgumentNullException(nameof(variables));
      }
      Variables = variables;
      if (methods == null) {
        throw new ArgumentNullException(nameof(methods));
      }
      Methods = methods;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = $"class {Identifier }";
      if (BaseClass != null) {
        text += $": {BaseClass}";
      }
      text += "{\r\n";
      foreach (var variable in Variables) {
        text += $"{variable}\r\n";
      }
      foreach (var method in Methods) {
        text += $"{method}\r\n";
      }
      text += "}";
      return text;
    }
  }
}
