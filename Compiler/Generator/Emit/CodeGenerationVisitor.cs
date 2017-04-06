using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;
using System;

namespace RappiSharp.Compiler.Generator.Emit {
  internal class CodeGenerationVisitor : Visitor {
    private readonly SymbolTable _symbolTable;
    private readonly MethodSymbol _method;
    private readonly ILAssembler _assembler;

        public CodeGenerationVisitor(SymbolTable symbolTable, MethodSymbol method, ILAssembler assembler) {
          _symbolTable = symbolTable;
          _method = method;
          _assembler = assembler;
        }

        public override void Visit(ArrayCreationNode node)
        {
            base.Visit(node);

            var type = _symbolTable.FindType(node);

            _assembler.Emit(OpCode.newarr, type);
        }


        public override void Visit(IntegerLiteralNode node)
        {
            base.Visit(node);

            _assembler.Emit(OpCode.ldc_i, (int)node.Value);
        }

        public override void Visit(CharacterLiteralNode node)
        {
            base.Visit(node);

            _assembler.Emit(OpCode.ldc_c, node.Value);
        }

        public override void Visit(StringLiteralNode node)
        {
            base.Visit(node);

            _assembler.Emit(OpCode.ldc_s, node.Value);
        }


        public override void Visit(WhileStatementNode node)
        {
            var startLabel = _assembler.CreateLabel();
            var endLabel = _assembler.CreateLabel();

            _assembler.SetLabel(startLabel);

            node.Condition.Accept(this);

            _assembler.Emit(OpCode.brfalse, endLabel);

            node.Body.Accept(this);

            _assembler.SetLabel(endLabel);
        }

        public override void Visit(IfStatementNode node)
        {
            var elseLabel = _assembler.CreateLabel();
            var endLabel = _assembler.CreateLabel();

            node.Condition.Accept(this);

            if(node.Else != null)
            {
                _assembler.Emit(OpCode.brfalse, elseLabel);
            }else
            {
                _assembler.Emit(OpCode.brfalse, endLabel);
            }
            node.Then.Accept(this);
            _assembler.Emit(OpCode.br, endLabel);
            _assembler.SetLabel(elseLabel);
            node.Else?.Accept(this);
            _assembler.SetLabel(endLabel);
        }

        public override void Visit(BasicDesignatorNode node)
        {
            if(node.Identifier == "true")
            {
                _assembler.Emit(OpCode.ldc_b, true);
            }

            if(node.Identifier == "false")
            {
                _assembler.Emit(OpCode.ldc_b, false);
            }
        }

        public override void Visit(AssignmentNode node)
        {
            base.Visit(node);
            var target = _symbolTable.GetTarget(node.Left);
            if(target is ParameterSymbol)
            {
                var index = _method.Parameters.IndexOf((ParameterSymbol)target);
                _assembler.Emit(OpCode.starg, index);
            } else if (target is LocalVariableSymbol)
            {
                var index = _method.LocalVariables.IndexOf((LocalVariableSymbol)target);
                _assembler.Emit(OpCode.stloc, index);
            } else
            {
                _assembler.Emit(OpCode.stfld, target);
            }
            //TODO how do i figure out whether it's an array assignment?

        }

        public override void Visit(MethodCallNode node)
        {
            var method = (MethodSymbol) _symbolTable.GetTarget(node.Designator);
            var builtInIndex = _symbolTable.Compilation.Methods.IndexOf(method);
            if(builtInIndex > -1)
            {
                base.Visit(node);
                //_assembler.Emit(OpCode.call, -(builtInIndex +1)); //HOW WAS I SUPPOSED TO KNOW!?
                _assembler.Emit(OpCode.call, method);
            } else
            {
                _assembler.Emit(OpCode.ldthis);
                base.Visit(node);
                _assembler.Emit(OpCode.callvirt, method);
            }
        }
    }
}
