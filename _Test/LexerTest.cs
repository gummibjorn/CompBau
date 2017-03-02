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
        private Location LOCATION = new Location(0, 0);

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

            Assert.IsInstanceOfType(_lexer.Next(), typeof(ErrorToken));

            Assert.IsTrue(Diagnosis.HasErrors);
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

        [TestMethod]
        public void NameKeywordClass()
        {
            initializeLexer("class");
            AssertNextFixToken(Tag.Class);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordElse()
        {
            initializeLexer("else");
            AssertNextFixToken(Tag.Else);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordIf()
        {
            initializeLexer("if");
            AssertNextFixToken(Tag.If);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordIs()
        {
            initializeLexer("is");
            AssertNextFixToken(Tag.Is);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordNew()
        {
            initializeLexer("new");
            AssertNextFixToken(Tag.New);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordReturn()
        {
            initializeLexer("return");
            AssertNextFixToken(Tag.Return);
            AssertEnd();
        }

        [TestMethod]
        public void NameKeywordWhile()
        {
            initializeLexer("while");
            AssertNextFixToken(Tag.While);
            AssertEnd();
        }

        [TestMethod]
        public void NameSingleLetter()
        {
            initializeLexer("x");
            AssertNextIdentifier("x");
            AssertEnd();
        }

        [TestMethod]
        public void NameWithNumbers()
        {
            initializeLexer("variable1");
            AssertNextIdentifier("variable1");
            AssertEnd();
        }

        [TestMethod]
        public void NameNormal()
        {
            initializeLexer("prettyHans");
            AssertNextIdentifier("prettyHans");
            AssertEnd();
        }

        [TestMethod]
        public void StringUnterminated()
        {
            initializeLexer("\"look ma, no end");
            AssertNextError();
            AssertEnd();
        }

        [TestMethod]
        public void StringEmpty()
        {
            initializeLexer("\"\"");
            AssertNextString("");
            AssertEnd();
        }

        [TestMethod]
        public void String()
        {
            initializeLexer("\"prettyHans\"");
            AssertNextString("prettyHans");
            AssertEnd();
        }

        private void AssertNextError()
        {
            Assert.IsInstanceOfType(_lexer.Next(), typeof(ErrorToken));
        }

        private void AssertNextString(string content) {
            AssertNext(new StringToken(LOCATION, content));
        }

        private void AssertNextIdentifier(string name) {
            AssertNext(new IdentifierToken(LOCATION, name));
        }

        private void AssertNextFixToken(Tag t)
        {
            AssertNext(new FixToken(LOCATION, t));
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
