using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Parser.Tree {
  internal class MethodNode : Node {
    public TypeNode ReturnType { get; }
    public string Identifier { get; }
    public List<VariableNode> Parameters { get; }
    public StatementBlockNode Body { get; } 

    public MethodNode(Location location, TypeNode returnType, string identifier, 
        List<VariableNode> parameters, StatementBlockNode body) :
      base(location) {
      if (returnType == null) {
        throw new ArgumentNullException(nameof(returnType));
      }
      ReturnType = returnType;
      if (string.IsNullOrEmpty(identifier)) {
        throw new ArgumentException("null or empty string", nameof(identifier));
      }
      Identifier = identifier;
      if (parameters == null) {
        throw new ArgumentNullException(nameof(parameters));
      }
      Parameters = parameters;
      if (body == null) {
        throw new ArgumentNullException(nameof(body));
      }
      Body = body;
    }

    public override void Accept(Visitor visitor) {
      visitor.Visit(this);
    }

    public override string ToString() {
      var text = $"{ReturnType} {Identifier} (";
      for (int index = 0; index < Parameters.Count; index++) {
        text += Parameters[index];
        if (index < Parameters.Count - 1) {
          text += ", ";
        }
      }
      text += $"){Body}";
      return text;
    }
  }
}
