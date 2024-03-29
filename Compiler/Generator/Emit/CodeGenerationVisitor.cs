﻿using RappiSharp.Compiler.Checker.General;
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
        private int _designator_level = 0;

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

        public override void Visit(ObjectCreationNode node)
        {
            //base.Visit(node);
            var type = _symbolTable.FindType(node);
            _assembler.Emit(OpCode.newobj, type);

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
                        base.Visit(node);
                        var type = _symbolTable.Find(_method, ((BasicDesignatorNode)node.Right).Identifier);
                        _assembler.Emit(OpCode.isinst, type);
                        break;
                }
            });
        }

        public override void Visit(TypeCastNode node)
        {
            base.Visit(node);
            var type = _symbolTable.Find(_method, node.Type.Identifier);
            _assembler.Emit(OpCode.castclass, type);
        }

        public override void Visit(WhileStatementNode node)
        {
            var startLabel = _assembler.CreateLabel();
            var endLabel = _assembler.CreateLabel();

            _assembler.SetLabel(startLabel);

            Expression(()=>node.Condition.Accept(this));

            _assembler.Emit(OpCode.brfalse, endLabel);

            node.Body.Accept(this);

            _assembler.Emit(OpCode.br, startLabel);

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
                else if (target is FieldSymbol)
                {
                    var classScope = (ClassSymbol)target.Scope;
                    //_assembler.Emit(OpCode.ldthis);
                    _assembler.Emit(OpCode.ldfld, classScope.AllFields.IndexOf(target as FieldSymbol));
                } else 
                {
                    //FIXME why does throwing an exception here break so many tests? it shouldn't go into this branch!
                    //throw new NotImplementedException();
                }
        }

        private void Load(ElementAccessNode node)
        {
            var target = _symbolTable.GetTarget(node.Designator);
            Load(target); //array instance
            Expression(() => node.Expression.Accept(this)); //index
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
                else if (node.Identifier == "null")
                {
                    _assembler.Emit(OpCode.ldnull, false); //FIXME does this need the false?
                }
                else if (node.Identifier == "this")
                {
                    _assembler.Emit(OpCode.ldthis);
                }
                else
                {
                    var target = _symbolTable.Find(_method, node.Identifier);
                    if(target is MethodSymbol)
                    {
                        var isBuiltIn = _symbolTable.Compilation.Methods.IndexOf((MethodSymbol)target) != -1;
                        if (!isBuiltIn)
                        {
                            _assembler.Emit(OpCode.ldthis);
                        }
                    }
                    if(target is FieldSymbol) 
                    {
                        _assembler.Emit(OpCode.ldthis);
                    }
                    Load(target);
                }
            }
        }

        public override void Visit(MemberAccessNode node)
        {
            _designator_level++;
            _expression_level++;
            base.Visit(node);
            _designator_level--;
            _expression_level--;

//            var target = _symbolTable.GetTarget(node.Designator);
            var type = _symbolTable.FindType(node.Designator);
            if(type is ArrayTypeSymbol)
            {
                if(node.Identifier == "length")
                {
                    _assembler.Emit(OpCode.ldlen);
                }
            } else
            {
                var ct = (ClassSymbol)type;
                var field = ct.AllFields.FindLast(f => f.Identifier == node.Identifier);

                if(_designator_level > 0)
                {
                    _assembler.Emit(OpCode.ldfld, ct.AllFields.IndexOf(field));
                }else
                {
                    if(_expression_level > 0)
                    {
                        if(ct.AllFields.IndexOf(field) != -1) //else its a method call
                        {
                            _assembler.Emit(OpCode.ldfld, ct.AllFields.IndexOf(field));
                        }
                    }else
                    {
                        //_assembler.Emit(OpCode.stfld, ct.Fields.IndexOf(field));
                    }
                }
            }
        }

        public override void Visit(ElementAccessNode node)
        {
            //ldelem: Pop index and array instance from stack and push value of the array element at index
            Expression(() => node.Designator.Accept(this));
            Expression(() => node.Expression.Accept(this)); //index
            if(_expression_level > 0)
            {
                _assembler.Emit(OpCode.ldelem);
            }
        }

        public override void Visit(AssignmentNode node)
        {
            node.Left.Accept(this);
            if(node.Left is ElementAccessNode)
            {
                //stelem: Pop value, index and array instance from stack and store value into the array element at index
                //Load((ElementAccessNode)node.Left); //array instance, index
                Expression(() => node.Right.Accept(this)); //value
                _assembler.Emit(OpCode.stelem);
            } else
            {
                var target = _symbolTable.GetTarget(node.Left);
                if(target is FieldSymbol)
                {
                    if(node.Left is BasicDesignatorNode)
                    {
                        _assembler.Emit(OpCode.ldthis);
                    }
                    Expression(() => node.Right.Accept(this));
                    var index = ((ClassSymbol)target.Scope).AllFields.IndexOf(target as FieldSymbol);
                    _assembler.Emit(OpCode.stfld, index);
                } else
                {
                    Expression(() => node.Right.Accept(this));
                    if(target is ParameterSymbol)
                    {
                        var index = _method.Parameters.IndexOf((ParameterSymbol)target);
                        _assembler.Emit(OpCode.starg, index);
                    } else if (target is LocalVariableSymbol)
                    {
                        var index = _method.LocalVariables.IndexOf((LocalVariableSymbol)target);
                        _assembler.Emit(OpCode.stloc, index);
                    }
                }
            }
        }

        public override void Visit(ReturnStatementNode node)
        {
            Expression(() => base.Visit(node));
            _assembler.Emit(OpCode.ret);
        }

        public override void Visit(MethodCallNode node)
        {
            var method = (MethodSymbol) _symbolTable.GetTarget(node.Designator);
            var isBuiltIn = _symbolTable.Compilation.Methods.IndexOf(method) != -1;
            if(isBuiltIn)
            {
                Expression(()=>base.Visit(node));
                _assembler.Emit(OpCode.call, method);
            } else
            {
                //_assembler.Emit(OpCode.ldthis);
                Expression(()=>base.Visit(node));
                _assembler.Emit(OpCode.callvirt, method);
            }
        }

        public override void Visit(UnaryExpressionNode node)
        {
            Expression(() =>
            {
                base.Visit(node);
                var op = node.Operator;

                switch (op)
                {
                    case Operator.Minus:
                    case Operator.Not:
                        _assembler.Emit(OpCode.neg);
                        break;
                }
            });
        }

    }
}
