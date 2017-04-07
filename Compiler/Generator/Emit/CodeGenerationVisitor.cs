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
        private int _expression_level = 0;

        private void Expression(Action action)
        {
            _expression_level += 1;
            action();
            _expression_level -= 1;
        }

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

        public override void Visit(BinaryExpressionNode node)
        {
            Expression(() =>
            {
                switch (node.Operator)
                {
                    case Operator.Or:
                        var endLabelOr = _assembler.CreateLabel();
                        var pushTrueLabel = _assembler.CreateLabel();

                        node.Left.Accept(this);

                        _assembler.Emit(OpCode.brtrue, pushTrueLabel);

                        node.Right.Accept(this);

                        _assembler.Emit(OpCode.brtrue, pushTrueLabel);

                        _assembler.Emit(OpCode.ldc_b, false);

                        _assembler.Emit(OpCode.br, endLabelOr);

                        _assembler.SetLabel(pushTrueLabel);

                        _assembler.Emit(OpCode.ldc_b, true);

                        _assembler.SetLabel(endLabelOr);

                        break;
                    case Operator.And:
                        var endLabelAnd = _assembler.CreateLabel();
                        var pushFalseLabel = _assembler.CreateLabel();

                        node.Left.Accept(this);

                        _assembler.Emit(OpCode.brfalse, pushFalseLabel);

                        node.Right.Accept(this);

                        _assembler.Emit(OpCode.brfalse, pushFalseLabel);

                        _assembler.Emit(OpCode.ldc_b, true);

                        _assembler.Emit(OpCode.br, endLabelAnd);

                        _assembler.SetLabel(pushFalseLabel);

                        _assembler.Emit(OpCode.ldc_b, false);

                        _assembler.SetLabel(endLabelAnd);

                        break;
                    case Operator.Divide:
                        base.Visit(node);
                        _assembler.Emit(OpCode.div);
                        break;
                    case Operator.Times:
                        base.Visit(node);
                        _assembler.Emit(OpCode.mul);
                        break;
                    case Operator.Modulo:
                        base.Visit(node);
                        _assembler.Emit(OpCode.mod);
                        break;
                    case Operator.Minus:
                        base.Visit(node);
                        _assembler.Emit(OpCode.sub);
                        break;
                    case Operator.Plus:
                        base.Visit(node);
                        _assembler.Emit(OpCode.add);
                        break;
                    case Operator.Less:
                        base.Visit(node);
                        _assembler.Emit(OpCode.clt);
                        break;
                    case Operator.LessEqual:
                        base.Visit(node);
                        _assembler.Emit(OpCode.cle);
                        break;
                    case Operator.Greater:
                        base.Visit(node);
                        _assembler.Emit(OpCode.cgt);
                        break;
                    case Operator.GreaterEqual:
                        base.Visit(node);
                        _assembler.Emit(OpCode.cge);
                        break;
                    case Operator.Equals:
                        base.Visit(node);
                        _assembler.Emit(OpCode.ceq);
                        break;
                    case Operator.Unequal:
                        base.Visit(node);
                        _assembler.Emit(OpCode.cne);
                        break;
                    case Operator.Is:
                        break;
                }
            });
        }

        public override void Visit(WhileStatementNode node)
        {
            var startLabel = _assembler.CreateLabel();
            var endLabel = _assembler.CreateLabel();

            _assembler.SetLabel(startLabel);

            Expression(()=>node.Condition.Accept(this));

            _assembler.Emit(OpCode.brfalse, endLabel);

            node.Body.Accept(this);

            _assembler.SetLabel(endLabel);
        }

        public override void Visit(IfStatementNode node)
        {
            var elseLabel = _assembler.CreateLabel();
            var endLabel = _assembler.CreateLabel();

            Expression(()=>node.Condition.Accept(this));

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

        private void Load(Symbol target)
        {
                if (target is LocalVariableSymbol)
                {
                    _assembler.Emit(OpCode.ldloc, _method.LocalVariables.IndexOf((LocalVariableSymbol)target));
                }
                else if (target is ParameterSymbol)
                {
                    _assembler.Emit(OpCode.ldarg, _method.Parameters.IndexOf((ParameterSymbol)target));
                }
                else
                {
                    //TODO handle fields, arrays
                    //throw new NotImplementedException();
                }
        }

        public override void Visit(BasicDesignatorNode node)
        {
            if (_expression_level > 0)
            {
                if (node.Identifier == "true")
                {
                    _assembler.Emit(OpCode.ldc_b, true);
                }
                else if (node.Identifier == "false")
                {
                    _assembler.Emit(OpCode.ldc_b, false);
                }
                else
                {
                    var target = _symbolTable.Find(_method, node.Identifier);
                    Load(target);
                }
            }
        }

        public override void Visit(MemberAccessNode node)
        {
            //base.Visit(node);
            var target = _symbolTable.GetTarget(node.Designator);
            var type = _symbolTable.FindType(node.Designator);
            if(type is ArrayTypeSymbol)
            {
                if(node.Identifier == "length")
                {
                    Load(target);
                    _assembler.Emit(OpCode.ldlen);
                }
            }
        }

        public override void Visit(ElementAccessNode node)
        {
            //ldelem: Pop index and array instance from stack and push value of the array element at index
            if(_expression_level > 0)
            {
                Load(node);
                _assembler.Emit(OpCode.ldelem);
            }

        }

        private void Load(ElementAccessNode node)
        {
            var target = _symbolTable.GetTarget(node.Designator);
            Load(target); //array instance
            Expression(() => node.Expression.Accept(this)); //index
        }

        public override void Visit(AssignmentNode node)
        {
            //node.Left.Accept(this);
            if(node.Left is ElementAccessNode)
            {
                //stelem: Pop value, index and array instance from stack and store value into the array element at index
                Load((ElementAccessNode)node.Left); //array instance, index
                Expression(() => node.Right.Accept(this)); //value
                _assembler.Emit(OpCode.stelem);
            } else
            {
                Expression(() => node.Right.Accept(this));
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
            }
            //TODO how do i figure out whether it's an array assignment?
        }

        public override void Visit(ReturnStatementNode node)
        {
            Expression(() => base.Visit(node));
        }

        public override void Visit(MethodCallNode node)
        {
            var method = (MethodSymbol) _symbolTable.GetTarget(node.Designator);
            var builtInIndex = _symbolTable.Compilation.Methods.IndexOf(method);
            if(builtInIndex > -1)
            {
                Expression(()=>base.Visit(node));
                _assembler.Emit(OpCode.call, method);
            } else
            {
                _assembler.Emit(OpCode.ldthis);
                Expression(()=>base.Visit(node));
                _assembler.Emit(OpCode.callvirt, method);
            }
        }

        public override void Visit(UnaryExpressionNode node)
        {
            Expression(() => base.Visit(node));
        }
    }
}
