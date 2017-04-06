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

namespace _Test
{
    [TestClass]
    public class GeneratorTest : AbstractTest
    {
        protected override string getPathInProject() { return "Generator"; }

        private RappiGenerator _generator;

        private ILIterator initializeGenerator(RappiLexer lexer)
        {
            _generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
            return new ILIterator(_generator.Metadata.Methods[_generator.Metadata.MainMethod].Code);
        }

        private ILIterator initializeGenerator()
        {
            return initializeGenerator(makeLexer());
        }

        private ILIterator initializeGenerator(string code)
        {
            return initializeGenerator(makeLexer(code));
        }

        private string expression(string type, string expr)
        {
            return main($"{type} a; a = {expr};");
        }

        private string program(string content)
        {
            return "class Program{ void Main(){} "+content+ "}";

        }

        private ILIterator method(string name)
        {
            foreach(var method in _generator.Metadata.Methods)
            {
                if(method.Identifier == name)
                {
                    return new ILIterator(method.Code);
                }
            }
            Assert.Fail($"Method {name} not found");
            return null;
        }

        [TestCleanup]
        public override void tearDown()
        {
            base.tearDown();
            //if serialization fails, we messed up
            var serializer = new XmlSerializer(typeof(Metadata));
            var writer = new StringWriter();
            serializer.Serialize(writer, _generator.Metadata);
        }

        [TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator(expression("int[]", "new int[10]"))
                .Next(ldc_i, 10)
                .Next(newarr)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignLocal()
        {
            initializeGenerator(main("int a; a=10;"))
                .Next(ldc_i, 10)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignParam()
        {
            initializeGenerator(program("void A(int i){ i = 10; }"));
            method("A")
                .Next(ldc_i, 10)
                .Next(starg, 0);
        }
    }
}