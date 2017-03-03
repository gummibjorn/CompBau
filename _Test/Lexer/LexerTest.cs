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
        private Location ZEROLOCATION = new Location(1, 1);

        public TestContext TestContext { get; set; }

        private void initializeLexer()
        {
            _lexer = new RappiLexer(File.OpenText("../../Lexer/" + TestContext.TestName + ".txt"));
        }

        private void initializeLexer(string inputString)
        {
            _lexer = new RappiLexer(new StringReader(inputString));
        }

        [TestMethod]
        public void ReadInteger_Overflow_Test() {
            initializeLexer("2147483648");


            AssertNextError(ZEROLOCATION);
        }

        [TestMethod]
        public void ReadInteger_Test() {
            initializeLexer("2147483647");

            AssertNext(new IntegerToken(ZEROLOCATION, 2147483647));
        }

        [TestMethod]
        public void CommentBlock_Test()
        {
            initializeLexer("/* testcomment */");

            AssertNext(new FixToken(new Location(1,18), Tag.End));
        }

        [TestMethod]
        public void SinglelineComment_Test()
        {
            initializeLexer("// testcomment");

            AssertNext(new FixToken(new Location(1,15), Tag.End));
        }

        [TestMethod]
        public void MultilineComment_Test()
        {
            initializeLexer();

            AssertNext(new IntegerToken(new Location(6,1), 10));
            AssertNext(new FixToken(new Location(6,3), Tag.End));
        }

        [TestMethod]
        public void EmptyFile()
        {
            initializeLexer();

            AssertNext(new FixToken(new Location(1,1), Tag.End));
        }

        [TestMethod]
        public void NameKeywordClass()
        {
            initializeLexer("class");
            AssertNextFixToken(Tag.Class);
        }

        [TestMethod]
        public void NameKeywordElse()
        {
            initializeLexer("else");
            AssertNextFixToken(Tag.Else);
        }

        [TestMethod]
        public void NameKeywordIf()
        {
            initializeLexer("if");
            AssertNextFixToken(Tag.If);
        }

        [TestMethod]
        public void NameKeywordIs()
        {
            initializeLexer("is");
            AssertNextFixToken(Tag.Is);
        }

        [TestMethod]
        public void NameKeywordNew()
        {
            initializeLexer("new");
            AssertNextFixToken(Tag.New);
        }

        [TestMethod]
        public void NameKeywordReturn()
        {
            initializeLexer("return");
            AssertNextFixToken(Tag.Return);
        }

        [TestMethod]
        public void NameKeywordWhile()
        {
            initializeLexer("while");
            AssertNextFixToken(Tag.While);
        }

        [TestMethod]
        public void NameSingleLetter()
        {
            initializeLexer("x");
            AssertNextIdentifier("x");
        }

        [TestMethod]
        public void NameWithNumbers()
        {
            initializeLexer("variable1");
            AssertNextIdentifier("variable1");
        }

        [TestMethod]
        public void NameNormal()
        {
            initializeLexer("prettyHans");
            AssertNextIdentifier("prettyHans");
        }

        [TestMethod]
        public void StringUnterminated()
        {
            initializeLexer("\"look ma, no end");
            AssertNextError(ZEROLOCATION);
        }

        [TestMethod]
        public void StringEmpty()
        {
            initializeLexer("\"\"");
            AssertNextString("");
        }

        [TestMethod]
        public void String()
        {
            initializeLexer("\"prettyHans\"");
            AssertNextString("prettyHans");
        }

        private void AssertNextError(Location expected)
        {
            var actual = _lexer.Next();

            Assert.IsInstanceOfType(actual, typeof(ErrorToken));

            Assert.AreEqual(expected, actual.Location);

            Assert.IsTrue(Diagnosis.HasErrors);
        }

        private void AssertNextString(string content) {
            AssertNext(new StringToken(ZEROLOCATION, content));
        }

        private void AssertNextIdentifier(string name) {
            AssertNext(new IdentifierToken(ZEROLOCATION, name));
        }

        private void AssertNextFixToken(Tag t)
        {
            AssertNext(new FixToken(ZEROLOCATION, t));
        }

        private void AssertNext(Token expected)
        {
            var actual = _lexer.Next();
            Assert.AreEqual(expected.Location, actual.Location);
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }
    }
}
