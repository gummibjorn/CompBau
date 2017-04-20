﻿using System;
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
                case OpCode.ldloc:
                    Stack.Push(Locals[Verify<int>(operand)]);
                    break;
                case OpCode.ldc_s:
                    Stack.Push(Verify<string>(operand));
                    break;
                case OpCode.ldc_i:
                    Stack.Push(Verify<int>(operand));
                    break;
                case OpCode.ldc_c:
                    Stack.Push(Verify<char>(operand));
                    break;
                case OpCode.call:
                    Call(operand);
                    break;
                case OpCode.ret:
                    _callStack.Pop();
                    break;
            }
        }

        private void Call(object Operand)
        {
                if(Operand == MethodDescriptor.WriteString)
                {
                    var arg = Stack.Pop<string>();
                    _console.Write(arg);
                }

                if(Operand == MethodDescriptor.WriteChar)
                {
                    var arg = Stack.Pop<char>();
                    _console.Write(arg);
                }

                if(Operand == MethodDescriptor.WriteInt)
                {
                    var arg = Stack.Pop<int>();
                    _console.Write(arg);
                }

                if(Operand == MethodDescriptor.ReadString)
                {
                    string input = _console.ReadLine();
                    Stack.Push(input);
                }

                if(Operand == MethodDescriptor.ReadChar)
                {
                    char input = (char) _console.Read();
                    Stack.Push(input);
                }

                if(Operand == MethodDescriptor.ReadInt)
                {
                    int input = int.Parse(_console.ReadLine());
                    Stack.Push(input);
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
