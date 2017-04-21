using System;
using RappiSharp.IL;
using RappiSharp.VirtualMachine.Descriptors;
using RappiSharp.VirtualMachine.Error;

namespace RappiSharp.VirtualMachine.Runtime
{
    internal class Interpreter
    {
        private readonly Loader _loader;
        private readonly CallStack _callStack;
        private IConsole _console;

        public Interpreter(Metadata metadata) : this(metadata, new SystemConsole()) { }

        public Interpreter(Metadata metadata, IConsole console)
        {
            _console = console;
            _loader = new Loader(metadata);
            _callStack = new CallStack();
        }

        public void Run()
        {
            SetProgramEntry();
            Interpret();
        }

        private void SetProgramEntry()
        {
            var mainMethod = _loader.MainMethod;
            var mainClass = mainMethod.Parent;
            if (mainMethod.ReturnType != null || mainMethod.ParameterTypes.Length != 0)
            {
                throw new InvalidILException("Invalid main method");
            }
            var mainObject = NewObject(mainClass);
            var locals = InitializedVariables(mainMethod.LocalTypes);
            _callStack.Push(new ActivationFrame(mainMethod, mainObject, new object[0], locals));
        }

        private object NewObject(ClassDescriptor mainClass)
        {
            // TODO: Implement object orientation later
            return null;
        }

        private void Interpret()
        {

            while (_callStack.Count != 0)
            {
                var instruction = ActiveFrame.Method.Code[InstructionPointer];
                InstructionPointer++;
                Execute(instruction);
            }
        }

        private void Execute(Instruction instruction)
        {
            var operand = instruction.Operand;
            switch (instruction.OpCode)
            {
                case OpCode.ldc_b:
                    Stack.Push(Verify<bool>(operand));
                    break;
                case OpCode.ldc_c:
                    Stack.Push(Verify<char>(operand));
                    break;
                case OpCode.ldc_i:
                    Stack.Push(Verify<int>(operand));
                    break;
                case OpCode.ldc_s:
                    Stack.Push(Verify<string>(operand));
                    break;
                case OpCode.ldnull:
                    break;
                case OpCode.br:
                    InstructionPointer += Verify<int>(operand);
                    break;
                case OpCode.brtrue:
                    if (Verify<bool>(Stack.Pop()))
                        InstructionPointer += Verify<int>(operand);
                    break;
                case OpCode.brfalse:
                    if (!Verify<bool>(Stack.Pop()))
                        InstructionPointer += Verify<int>(operand);
                    break;
                case OpCode.neg:
                    Neg();
                    break;
                case OpCode.add:
                    BinaryOp((a, b) => a + b);
                    break;
                case OpCode.sub:
                    BinaryOp((a, b) => a - b);
                    break;
                case OpCode.mul:
                    BinaryOp((a, b) => a * b);
                    break;
                case OpCode.div:
                    BinaryOp((a, b) => a / b);
                    break;
                case OpCode.mod:
                    BinaryOp((a, b) => a % b);
                    break;
                case OpCode.clt:
                    BinaryOp((a, b) => a < b);
                    break;
                case OpCode.cle:
                    BinaryOp((a, b) => a <= b);
                    break;
                case OpCode.cgt:
                    BinaryOp((a, b) => a > b);
                    break;
                case OpCode.cge:
                    BinaryOp((a, b) => a >= b);
                    break;
                case OpCode.ceq:
                    BinaryOp((a, b) => a == b);
                    break;
                case OpCode.cne:
                    BinaryOp((a, b) => a != b);
                    break;
                case OpCode.ldloc:
                    Stack.Push(Locals[Verify<int>(operand)]);
                    break;
                case OpCode.stloc:
                    Stloc(Verify<int>(operand));
                    break;
                case OpCode.ldarg:
                    Stack.Push(Arguments[Verify<int>(operand)]);
                    break;
                case OpCode.starg:
                    Starg(Verify<int>(operand));
                    break;
                case OpCode.ldfld:
                    break;
                case OpCode.stfld:
                    break;
                case OpCode.newarr:
                    break;
                case OpCode.ldlen:
                    break;
                case OpCode.ldelem:
                    break;
                case OpCode.stelem:
                    break;
                case OpCode.call:
                    Call(operand);
                    break;
                case OpCode.newobj:
                    break;
                case OpCode.ldthis:
                    break;
                case OpCode.callvirt:
                    CallVirt(Verify<MethodDescriptor>(operand));
                    break;
                case OpCode.isinst:
                    break;
                case OpCode.castclass:
                    break;
                case OpCode.ret:
                    _callStack.Pop();
                    break;
            }
        }

        private void CallVirt(MethodDescriptor method)
        {
            var locals = InitializedVariables(method.LocalTypes);
            var args = new object[method.ParameterTypes.Length];
            for(var i = method.ParameterTypes.Length -1; i >= 0; i--)
            {
                var type = method.ParameterTypes[i];
                args[i] = Verify(Stack.Pop(), type);

            }
            //var thisReference = Stack.Pop();
            object thisReference = null;
            var frame = new ActivationFrame(method, thisReference, args, locals);
            _callStack.Push(frame);
        }

        private void BinaryOp<T>(Func<int,int,T> action)
        {
            var right = Stack.Pop<int>();
            var left = Stack.Pop<int>();
            Stack.Push(action(left, right));
        }

        private void Neg()
        {
            var a = Stack.Pop();
            if (a is bool)
            {
                Stack.Push(!Verify<bool>(a));
            } else
            {
                Stack.Push(-Verify<int>(a));
            }
        }

        private void Starg(int index)
        {
            var value = Stack.Pop();
            var localType = ActiveFrame.Method.ParameterTypes[index];
            Arguments[index] = Verify(value, localType);
        }

        private void Stloc(int index)
        {
            var value = Stack.Pop();
            var localType = ActiveFrame.Method.LocalTypes[index];
            Locals[index] = Verify(value, localType);
        }

        private object Verify(object value, TypeDescriptor type)
        {
            if(type == InbuiltType.Bool)
            {
                return Verify<bool>(value);
            }else if(type == InbuiltType.Int)
            {
                return Verify<int>(value);
            }else if(type == InbuiltType.String)
            {
                return Verify<string>(value);
            }else if(type == InbuiltType.Char)
            {
                return Verify<char>(value);
            }else
            {
                throw new InvalidILException($"Expected {type}");
            }
        }

        private void Call(object operand)
        {
                if(operand == MethodDescriptor.WriteString)
                {
                    var arg = Stack.Pop<string>();
                    _console.Write(arg);
                }

                if(operand == MethodDescriptor.WriteChar)
                {
                    var arg = Stack.Pop<char>();
                    _console.Write(arg);
                }

                if(operand == MethodDescriptor.WriteInt)
                {
                    var arg = Stack.Pop<int>();
                    _console.Write(arg);
                }

                if(operand == MethodDescriptor.ReadString)
                {
                    string input = _console.ReadLine();
                    Stack.Push(input);
                }

                if(operand == MethodDescriptor.ReadChar)
                {
                    char input = (char) _console.Read();
                    Stack.Push(input);
                }

                if(operand == MethodDescriptor.ReadInt)
                {
                    int input = int.Parse(_console.ReadLine());
                    Stack.Push(input);
                }

                if(operand == MethodDescriptor.Halt)
                {
                    var arg = Stack.Pop<string>();
                    _console.Write(arg);
                    Environment.Exit(1);
                }
        }



        private object[] InitializedVariables(TypeDescriptor[] types)
        {
            var variables = new object[types.Length];
            for (int index = 0; index < variables.Length; index++)
            {
                variables[index] = DefaultValue(types[index]);
            }
            return variables;
        }

        private object DefaultValue(TypeDescriptor type)
        {
            if (type is InbuiltType)
            {
                if (type == InbuiltType.Bool)
                {
                    return false;
                }
                if (type == InbuiltType.Char)
                {
                    return '\0';
                }
                if (type == InbuiltType.Int)
                {
                    return 0;
                }
                if (type == InbuiltType.String)
                {
                    return string.Empty;
                }
                throw new InvalidILException("Invalid inbuilt type");
            }
            else
            {
                return null;
            }
        }

        private ActivationFrame ActiveFrame
        {
            get
            {
                return _callStack.Peek();
            }
        }

        private MethodDescriptor ActiveMethod
        {
            get
            {
                return ActiveFrame.Method;
            }
        }

        private object[] Locals
        {
            get
            {
                return ActiveFrame.Locals;
            }
        }

        private object[] Arguments
        {
            get
            {
                return ActiveFrame.Arguments;
            }
        }

        private EvaluationStack Stack
        {
            get
            {
                return ActiveFrame.EvaluationStack;
            }
        }

        private object ThisReference
        {
            get
            {
                return ActiveFrame.ThisReference;
            }
        }

        private int InstructionPointer
        {
            get
            {
                return ActiveFrame.InstructionPointer;
            }
            set
            {
                ActiveFrame.InstructionPointer = value;
            }
        }

        private T Verify<T>(object value)
        {
            if (!(value is T))
            {
                throw new InvalidILException($"Expected {typeof(T)} instead of {value}");
            }
            return (T)value;
        }
    }
}
