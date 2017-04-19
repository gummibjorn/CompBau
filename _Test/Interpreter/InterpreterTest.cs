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

        private void initializeInterpreter(RappiLexer lexer)
        {
            var generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
            if (Diagnosis.HasErrors)
            {
                Assert.Fail("Error while generating code: " + Diagnosis.Messages);
            }
            _interpreter = new Interpreter(generator.Metadata);
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

        private string program(string content)
        {
            return "class Program{ " + content + "}";

        }

        [TestMethod]
        public void Hello()
        {
        }
    }
}