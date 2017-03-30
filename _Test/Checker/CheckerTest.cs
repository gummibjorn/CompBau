using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Lexer;
using System.IO;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler;

namespace _Test
{
    [TestClass]
    public class CheckerTest
    {
        private RappiChecker _checker;

        private void initializeChecker(string code)
        {
            var lexer = new RappiLexer(new StringReader(code));
            var parser = new RappiParser(lexer);
            _checker = new RappiChecker(parser.ParseProgram());
        }

        [TestInitialize]
        public void setUp()
        {
            Diagnosis.Clear();
        }

        [TestCleanup]
        public void tearDown()
        {
            if (Diagnosis.HasErrors)
            {
                Assert.Fail(Diagnosis.Messages);
            }
        }

        private string main(string content)
        {
            return "class Prorahm{ void Main(){" + content + "}}";
        }

        [TestMethod]
        public void AssignmentBinaryExpressionBool()
        {
            initializeChecker(main("bool a; a = true && true;"));
        }
    }
}
