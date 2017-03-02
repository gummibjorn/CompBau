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
                initializeLexer("4294967296");

                Assert.IsNull(_lexer.Next());
        }

        [TestMethod]
        public void ReadInteger_Test() {
                initializeLexer("4294967295");

                AssertNext(new IntegerToken(new Location(0,0), 4294967295));
        }


        [TestMethod]
        public void EmptyFile()
        {
            initializeLexer();

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
