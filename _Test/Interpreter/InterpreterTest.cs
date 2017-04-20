﻿using _Test.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler.Generator;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;
using System;
using System.Collections.Generic;
using static RappiSharp.IL.OpCode;
using System.Xml.Serialization;
using System.IO;
using RappiSharp.VirtualMachine.Runtime;
using RappiSharp.Compiler;

namespace _Test
{
    [TestClass]
    public class InterpreterTest : AbstractTest
    {
        protected override string getPathInProject() { return "Interpreter"; }

        private Interpreter _interpreter;
        private TestConsole _console;
        private bool _didRun;

        private void initializeInterpreter(RappiLexer lexer)
        {
            var generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
            if (Diagnosis.HasErrors)
            {
                Assert.Fail("Error while generating code: " + Diagnosis.Messages);
            }
            _console = new TestConsole();
            _interpreter = new Interpreter(generator.Metadata, _console);
        }

        private void runInterpreter()
        {
            initializeInterpreter(makeLexer());
            Run();
        }

        private void runInterpreter(string code)
        {
            initializeInterpreter(makeLexer(code));
            Run();
        }

        private void initializeInterpreter()
        {
            initializeInterpreter(makeLexer());
        }

        private void initializeInterpreter(string code)
        {
            initializeInterpreter(makeLexer(code));
        }

        public void Run()
        {
            _didRun = true;
            _interpreter.Run();
        }

        [TestInitialize]
        public void Before()
        {
            _didRun = false;
        }

        [TestCleanup]
        public void After()
        {
            if (!_didRun)
            {
                Assert.Fail("Test didn't run!");
            }
        }

        class TestConsole : IConsole
        {

            private Queue<int> _input = new Queue<int>();
            private StringWriter _output = new StringWriter();

            public StringWriter Output
            {
                get { return _output; }
                set { _output = value; }
            }

            public int Read()
            {
                return _input.Dequeue();
            }

            public string ReadLine()
            {
                string s = "";
                int c = _input.Dequeue();
                while(c != '\n')
                {
                    s += (char)c;
                    c = _input.Dequeue();
                }
                return s;
            }

            public void Write(object o)
            {
                _output.Write(o);
            }

            public void Send(char input)
            {
                _input.Enqueue(input);
            }

            public void Send(string input)
            {
                foreach(var c in input.ToCharArray()){
                    _input.Enqueue(c);
                }
                _input.Enqueue('\n');
            }

        }

        private string program(string content)
        {
            return "class Program{ " + content + "}";

        }

        [TestMethod]
        public void WriteString()
        {
            runInterpreter(main("WriteString(\"Hello World\");"));
            Assert.AreEqual("Hello World", _console.Output.ToString());
        }

        [TestMethod]
        public void WriteChar()
        {
            runInterpreter(main("WriteChar('\n');"));
            Assert.AreEqual("\n", _console.Output.ToString());
        }

        [TestMethod]
        public void WriteInt()
        {
            runInterpreter(main("WriteInt(12);"));
            Assert.AreEqual("12", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadString()
        {
            initializeInterpreter(main("WriteString(ReadString());"));
            _console.Send("Hello Wurld");
            Run();
            Assert.AreEqual("Hello Wurld", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadChar()
        {
            initializeInterpreter(main("WriteChar(ReadChar());"));
            _console.Send('x');
            Run();
            Assert.AreEqual("x", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadInt()
        {
            initializeInterpreter(main("WriteInt(ReadInt());"));
            _console.Send("9001");
            Run();
            Assert.AreEqual("9001", _console.Output.ToString());
        }

        [TestMethod]
        public void LocalsLoadAndStore()
        {
            initializeInterpreter(main("int i; i = 12; WriteInt(i)"));
            _console.Send("9001");
            Run();
            Assert.AreEqual("9001", _console.Output.ToString());
        }

        [TestMethod]
        public void OpAdd()
        {
            runInterpreter(main("WriteInt(1+2);"));
            Assert.AreEqual("3", _console.Output.ToString());
        }

        [TestMethod]
        public void OpSub()
        {
            runInterpreter(main("WriteInt(1-2);"));
            Assert.AreEqual("-1", _console.Output.ToString());
        }

        [TestMethod]
        public void OpMul()
        {
            runInterpreter(main("WriteInt(2*4);"));
            Assert.AreEqual("8", _console.Output.ToString());
        }

        [TestMethod]
        public void OpDiv()
        {
            runInterpreter(main("WriteInt(15/3);"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void OpMod()
        {
            runInterpreter(main("WriteInt(17%5);"));
            Assert.AreEqual("2", _console.Output.ToString());
        }
    }
}