namespace RappiSharp.Compiler.Parser.Tree {
  internal abstract class Visitor {
    public virtual void Visit(ProgramNode node) {
      foreach (var classNode in node.Classes) {
        classNode.Accept(this);
      }
    }

    public virtual void Visit(ClassNode node) {
      foreach (var variableNode in node.Variables) {
        variableNode.Accept(this);
      }
      foreach (var methodNode in node.Methods) {
        methodNode.Accept(this);
      }
    }

    public virtual void Visit(VariableNode node) {
      node.Type.Accept(this);
    }

    public virtual void Visit(MethodNode node) {
      node.ReturnType?.Accept(this);
      foreach (var parameterNode in node.Parameters) {
        parameterNode.Accept(this);
      }
      node.Body.Accept(this);
    }

    public virtual void Visit(BasicTypeNode node) {
    }

    public virtual void Visit(ArrayTypeNode node) {
      node.ElementType.Accept(this);
    }

    public virtual void Visit(StatementBlockNode node) {
      foreach (var statementNode in node.Statements) {
        statementNode.Accept(this);
      }
    }

    public virtual void Visit(LocalDeclarationNode node) {
      node.Variable.Accept(this);
    }

    public virtual void Visit(AssignmentNode node) {
      node.Left.Accept(this);
      node.Right.Accept(this);
    }

    public virtual void Visit(IfStatementNode node) {
      node.Condition.Accept(this);
      node.Then.Accept(this);
      node.Else?.Accept(this);
    }

    public virtual void Visit(WhileStatementNode node) {
      node.Condition.Accept(this);
      node.Body.Accept(this);
    }

    public virtual void Visit(CallStatementNode node) {
      node.Call.Accept(this);
    }

    public virtual void Visit(ReturnStatementNode node) {
      node.Expression?.Accept(this);
    }

    public virtual void Visit(BinaryExpressionNode node) {
      node.Left.Accept(this);
      node.Right.Accept(this);
    }

    public virtual void Visit(UnaryExpressionNode node) {
      node.Operand.Accept(this);
    }

    public virtual void Visit(TypeCastNode node) {
      node.Type.Accept(this);
      node.Designator.Accept(this);
    }

    public virtual void Visit(ObjectCreationNode node) {
      node.Type.Accept(this);
    }

    public virtual void Visit(ArrayCreationNode node) {
      node.ElementType.Accept(this);
      node.Expression.Accept(this);
    }

    public virtual void Visit(MethodCallNode node) {
      node.Designator.Accept(this);
      foreach (var argumentNode in node.Arguments) {
        argumentNode.Accept(this);
      }
    }

    public virtual void Visit(BasicDesignatorNode node) {
    }

    public virtual void Visit(ElementAccessNode node) {
      node.Designator.Accept(this);
      node.Expression.Accept(this);
    }

    public virtual void Visit(MemberAccessNode node) {
      node.Designator.Accept(this);
    }

    public virtual void Visit(IntegerLiteralNode node) {
    }

    public virtual void Visit(StringLiteralNode node) {
    }

    public virtual void Visit(CharacterLiteralNode node) {
    }
  }
}
