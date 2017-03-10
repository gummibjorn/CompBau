using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using RappiSharp.Compiler.Parser;
using System.IO;

namespace _Test
{
    [TestClass]
    public class ParserTest {
        private RappiParser _parser;

        public TestContext TestContext { get; set; }

        private void initializeParser()
        {
            var lexer = new RappiLexer(File.OpenText("../../Parser/" + TestContext.TestName + ".txt"));
            _parser = new RappiParser(lexer);
        }

        private void initializeParser(string code)
        {
            var lexer = new RappiLexer(new StringReader(code));
            _parser = new RappiParser(lexer);
        }

        private void AssertDiagnosisContains(string msg)
        {
            Assert.IsTrue(Diagnosis.Messages.Contains(msg), $"Expected diagnosis to contain '${msg}', but was '${Diagnosis.Messages}'");
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

        [TestInitialize]
        public void setUp()
        {
            Diagnosis.Clear();
        }

        [TestMethod]
        public void ClassEmpty()
        {
            initializeParser("class Foo{}");
            _parser.ParseProgram();
        }

        [TestMethod]
        public void ClassNoname()
        {
            initializeParser("class{}");
            _parser.ParseProgram();
            AssertDiagnosisContains("Identifier expected");
        }
    }

}
