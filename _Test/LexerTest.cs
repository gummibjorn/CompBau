using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System;
using System.IO;

namespace _Test
{
    [TestClass]
    public class LexerTest
    {
        public TestContext TestContext { get; set; }

        RappiLexer _lexer;
        [TestInitialize]
        public void Before()
        {
            _lexer = new RappiLexer(File.OpenText("../.././LexerFiles/" + TestContext.TestName + ".txt"));
            //Console.Write(TestContext.TestName);
        }

        [TestMethod]
        public void EmptyFile()
        {
            AssertEnd();
        }

        private void AssertNext(Token t)
        {
            Assert.AreEqual(t.ToString(), _lexer.Next().ToString());

        }

        private void AssertEnd()
        {
            AssertNext(new FixToken(new Location(0,0), Tag.End));

        }


    }
}
