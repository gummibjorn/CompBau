﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System.IO;

namespace _Test
{
    [TestClass]
    public class LexerTest
    {

        RappiLexer _lexer;

        public TestContext TestContext { get; set; }

        private void initializeLexer()
        {
            _lexer = new RappiLexer(File.OpenText("../.././LexerFiles/" + TestContext.TestName + ".txt"));
        }

        private void initializeLexer(string inputString)
        {
            _lexer = new RappiLexer(new StringReader(inputString));
        }

        [TestMethod]
        public void ReadInteger_Overflow_Test() {
            initializeLexer("2147483648");

            var token = _lexer.Next();

            Assert.IsInstanceOfType(token, typeof(ErrorToken));

            Assert.AreEqual(new Location(0,0), token.Location);

            Assert.IsTrue(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ReadInteger_Test() {
            initializeLexer("2147483647");

            AssertNext(new IntegerToken(new Location(0,0), 2147483647));
        }

        [TestMethod]
        public void EmptyFile()
        {
            initializeLexer();

            AssertEnd();
        }

        private void AssertNext(Token t)
        {
            var token = _lexer.Next();
            Assert.AreEqual(t.Location, token.Location);
            Assert.AreEqual(t.ToString(), token.ToString());
        }

        private void AssertEnd()
        {
            AssertNext(new FixToken(new Location(0,0), Tag.End));
        }


    }
}
