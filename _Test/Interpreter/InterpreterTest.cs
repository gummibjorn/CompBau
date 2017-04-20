using _Test.Generator;
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

        private void initializeInterpreter(RappiLexer lexer)
        {
            var generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
            if (Diagnosis.HasErrors)
            {
                Assert.Fail("Error while generating code: " + Diagnosis.Messages);
            }
            _console = new TestConsole();
            _interpreter = new Interpreter(generator.Metadata, _console);
            _interpreter.Run();
        }

        private void initializeInterpreter()
        {
            initializeInterpreter(makeLexer());
        }

        private void initializeInterpreter(string code)
        {
            initializeInterpreter(makeLexer(code));
        }

        class TestConsole : IConsole
        {

            //TODO: add a way to provide input
            private Queue<int> _input = new Queue<int>();
            private StringWriter _output = new StringWriter();

            public StringWriter Output
            {
                get { return _output; }
                set { _output = value; }
            }

            public int Read()
            {
                if(_input.Count == 0)
                {
                    Assert.Fail("Reading from empty Queue!");
                }
                return _input.Dequeue();
            }

            public string ReadLine()
            {
                string s = "";
                int c = _input.Dequeue();
                while(c != '\n')
                {
                    s += (char)c;
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
        public void Hello()
        {
            initializeInterpreter(main("WriteString(\"Hello World\");"));
            Assert.AreEqual("Hello World", _console.Output.ToString());
        }
    }
}