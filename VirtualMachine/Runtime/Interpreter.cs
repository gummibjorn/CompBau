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
        public RawHeap _heap;

        private const int KB = 1024;
        private const int MB = 1024 * KB;
        private const int GB = 1024 * MB;

        public Interpreter(Metadata metadata) : this(metadata, new SystemConsole()) { }

        public Interpreter(Metadata metadata, IConsole console, int heapSize = 2 * KB)
        {
            _console = console;
            _loader = new Loader(metadata);
            _callStack = new CallStack();
            _heap = new RawHeap(heapSize, _callStack);
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

        private IntPtr NewObject(ClassDescriptor descriptor)
        {
            return _heap.Allocate(descriptor);
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
                    Stack.Push(_heap.Allocate(Verify<string>(operand)));
                    break;
                case OpCode.ldnull:
                    Stack.Push(IntPtr.Zero);
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
                    Stack.Push(Compare());
                    break;
                case OpCode.cne:
                    Stack.Push(!Compare());
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
                    LdFld(Verify<int>(operand));
                    break;
                case OpCode.stfld:
                    StFld(Verify<int>(operand));
                    break;
                case OpCode.newarr:
                    NewArr(Verify<ArrayDescriptor>(operand));
                    break;
                case OpCode.ldlen:
                    Stack.Push(Ldlen(Stack.Pop<IntPtr>()));
                    break;
                case OpCode.ldelem:
                    Ldelem();
                    break;
                case OpCode.stelem:
                    Stelem();
                    break;
                case OpCode.call:
                    Call(operand);
                    break;
                case OpCode.newobj:
                    Stack.Push(NewObject(Verify<ClassDescriptor>(operand)));
                    break;
                case OpCode.ldthis:
                    Stack.Push(ActiveFrame.ThisReference);
                    break;
                case OpCode.callvirt:
                    CallVirt(Verify<MethodDescriptor>(operand));
                    break;
                case OpCode.isinst:
                    Stack.Push(IsInst(Verify<ClassDescriptor>(operand)));
                    break;
                case OpCode.castclass:
                    CastClass(Verify<ClassDescriptor>(operand));
                    break;
                case OpCode.ret:
                    Ret();
                    break;
            }
        }

        private void NullReferenceCheck(IntPtr ptr)
        {
            if(ptr == IntPtr.Zero)
            {
                throw new VMException("Reference is null");
            }
        }

        private object Ldlen(IntPtr ptr)
        {
            NullReferenceCheck(ptr);
            return _heap.GetArrayLength(ptr);
        }

        private bool IsInst(ClassDescriptor op)
        {
            var ptr = Stack.Pop<IntPtr>();
            if(ptr == IntPtr.Zero)
            {
                return false;
            }
            var type = (ClassDescriptor)_heap.GetType(ptr);
            return type.BaseTypes[op.Level] == op;
        }

        private void StFld(int fieldIndex)
        {
            var value = Stack.Pop();
            var instance = Stack.Pop<IntPtr>();
            NullReferenceCheck(instance);
            var instanceType = (ClassDescriptor)_heap.GetType(instance);
            Verify(value, instanceType.FieldTypes[fieldIndex]);
            _heap.StoreField(instance, fieldIndex, value);
        }

        private void LdFld(int fieldIndex)
        {
            var instance = Stack.Pop<IntPtr>();
            NullReferenceCheck(instance);
            Stack.Push(_heap.LoadField(instance, fieldIndex));
        }

        private void Ldelem()
        {
            var index = Stack.Pop<int>();
            var ptr = Stack.Pop<IntPtr>();
            NullReferenceCheck(ptr);
            var type = (ArrayDescriptor)_heap.GetType(ptr);
            var element = _heap.LoadElement(ptr, index, type.ElementType);
            Stack.Push(element);
        }

        private void Stelem()
        {
            var value = Stack.Pop();
            var index = Stack.Pop<int>();
            var ptr = Stack.Pop<IntPtr>();
            NullReferenceCheck(ptr);
            var type = (ArrayDescriptor)_heap.GetType(ptr);
            Verify(value, type.ElementType);
            _heap.StoreElement(ptr, index, value, type.ElementType);
        }

        private void NewArr(ArrayDescriptor arrayDescriptor)
        {
            var length = Stack.Pop<int>();
            var ptr = _heap.Allocate(arrayDescriptor, length);
            Stack.Push(ptr);
        }

        private void Ret()
        {
            var type = ActiveFrame.Method.ReturnType;
            if(type != null)
            {
                var value = Verify(Stack.Pop(), type);
                _callStack.Pop();
                Stack.Push(value);
            } else
            {
                _callStack.Pop();
            }
        }

        private void CheckEvaluationStackEmpty()
        {
            if(Stack.Count != 0)
            {
                throw new VMException("evaluation stack is not empty after ret");
            }
        }

        private void CallVirt(MethodDescriptor staticMethod)
        {
            var args = new object[staticMethod.ParameterTypes.Length];
            for(var i = staticMethod.ParameterTypes.Length -1; i >= 0; i--)
            {
                var type = staticMethod.ParameterTypes[i];
                args[i] = Verify(Stack.Pop(), type);

            }
            var thisReference = Stack.Pop<IntPtr>();

            NullReferenceCheck(thisReference);
            var dynamicMethod = ((ClassDescriptor)_heap.GetType(thisReference)).VirtualTable[staticMethod.Position];
            var locals = InitializedVariables(dynamicMethod.LocalTypes);
            var frame = new ActivationFrame(dynamicMethod, thisReference, args, locals);
            _callStack.Push(frame);
        }

        private void CastClass(ClassDescriptor targetType)
        {
            var ptr = Stack.Pop<IntPtr>();

            if(ptr != IntPtr.Zero)
            {
                var type = (ClassDescriptor)_heap.GetType(ptr);

                if(ptr != IntPtr.Zero && type.BaseTypes[targetType.Level] != targetType)
                {
                    throw new VMException("Invalid cast");
                }
            }

            Stack.Push(ptr);
        }

        private void BinaryOp<T>(Func<int,int,T> action)
        {
            var right = Stack.Pop<int>();
            var left = Stack.Pop<int>();
            Stack.Push(action(left, right));
        }

        private bool Compare()
        {
            var right = Stack.Pop();
            var left = Stack.Pop();
            if(right is string)
            {
                right = _heap.Allocate((string)right);
            }

            if(left is string)
            {
                left = _heap.Allocate((string)left);
            }

            if(right != null)
            {
                return right.Equals(left);
            } else if (left != null)
            {
                return left.Equals(right);
            } else
            {
                return true;
            }
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
            Arguments[index] = value;
        }

        private void Stloc(int index)
        {
            var value = Stack.Pop();
            var localType = ActiveFrame.Method.LocalTypes[index];
            Verify(value, localType);
            Locals[index] = value;
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
                return Verify<int>(value);
            }else if(type == InbuiltType.Char)
            {
                return Verify<char>(value);
            }else {
                try
                {
                    if((IntPtr)value != IntPtr.Zero)
                    {
                        var valueType = _heap.GetType((IntPtr)value);
                        
                        if(valueType is ClassDescriptor)
                        {
                            if(type != ((ClassDescriptor)valueType).BaseTypes[((ClassDescriptor)type).Level])
                            {
                                throw new InvalidILException($"Expected {type} instead of {valueType}");
                            }
                        }else if(valueType is ArrayDescriptor)
                        {
                            if(valueType != type)
                            {
                                throw new InvalidILException($"Expected {type} instead of {valueType}");
                            }
                        }
                    }
                } catch (Exception e)
                {
                        throw new InvalidILException("Invalid type");
                }
                return value; 
            }
        }

        private void Call(object operand)
        {
                if(operand == MethodDescriptor.WriteString)
                {
                    var arg = Stack.Pop<int>();
                    _console.Write(_heap.LoadString(arg));
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
                    Stack.Push(_heap.Allocate(input));
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
                    var arg = Stack.Pop<int>();
                    _console.Write(_heap.LoadString(arg));
                    Environment.Exit(1);
                }
        }



        public static object[] InitializedVariables(TypeDescriptor[] types)
        {
            var variables = new object[types.Length];
            for (int index = 0; index < variables.Length; index++)
            {
                variables[index] = DefaultValue(types[index]);
            }
            return variables;
        }

        internal static object DefaultValue(TypeDescriptor type)
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
                    //index on default string
                    return 0;
                }
                throw new InvalidILException("Invalid inbuilt type");
            }
            else
            {
                return IntPtr.Zero;
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
                throw new InvalidILException($"Expected {typeof(T)} instead of {value.GetType()} ({value})");
            }
            return (T)value;
        }
    }
}
