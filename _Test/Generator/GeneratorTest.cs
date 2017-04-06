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

        [TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator(expression("int[]", "new int[10]"))
                .Next(ldc_i, 10)
                .Next(newarr)
                .Return();
        }

    }
}