﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System.IO;
using System;

namespace _Test
{
    [TestClass]
    public class LexerTest : AbstractTest
    {
        protected override string getPathInProject() { return "Lexer"; }

        private RappiLexer _lexer;

        private void initializeLexer()
        {
            _lexer = makeLexer();
        }

        private void initializeLexer(string code)
        {
            _lexer = makeLexer(code);
        }

        [TestMethod]
        public void ReadInteger_Negative_MAX_Test() {
            initializeLexer("2147483648");

            AssertNext(new IntegerToken(L, 2147483648));
        }

        [TestMethod]
        public void ReadInteger_Overflow_Test() {
            initializeLexer("2147483649");

            AssertNextError(L);
        }

        [TestMethod]
        public void ReadInteger_Huge_Overflow_Test() {
            initializeLexer("2147483649234123412341213412345565464561234123841789236478912364078126381723089471230847120893741089243456345634563456345634564");

            AssertNextError(L);
        }

        [TestMethod]
        public void ReadInteger_Integer() {
            initializeLexer("2147483649");

            AssertNextError(L);
        }

        [TestMethod]
        public void ReadInteger() {
            initializeLexer("21474");

            AssertNext(new IntegerToken(L, 21474));
        }

        [TestMethod]
        public void ReadInteger_Positive_MAX() {
            initializeLexer("2147483647");

            AssertNext(new IntegerToken(L, 2147483647));
        }

        [TestMethod]
        public void CommentBlock()
        {
            initializeLexer("/* testcomment */");

            AssertNext(new FixToken(new Location(1, 18), Tag.End));
        }

        [TestMethod]
        public void CommentBlockNotClosing()
        {
            initializeLexer("/* testcomment");

            AssertNext(new FixToken(new Location(1, 15), Tag.End));
            AssertDiagnosisContains("unclosed multiline");
        }

        [TestMethod]
        public void SinglelineComment()
        {
            initializeLexer("// testcomment");

            AssertNext(new FixToken(new Location(1, 15), Tag.End));
        }

        [TestMethod]
        public void MultilineComment()
        {
            initializeLexer();

            AssertNext(new IntegerToken(new Location(6, 1), 10));
            AssertNext(new FixToken(new Location(6, 3), Tag.End));
        }

        [TestMethod]
        public void EmptyFile()
        {
            initializeLexer();

            AssertNext(new FixToken(new Location(1, 1), Tag.End));
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
            AssertNextError(L);
        }

        [TestMethod]
        public void StringEmpty()
        {
            initializeLexer("\"\"");
            AssertNextString("");
        }

        [TestMethod]
        public void StringEscapes()
        {
            initializeLexer();
            AssertNext(new StringToken(null, "\n"));
            AssertNext(new StringToken(null, "\\"));
            AssertNext(new StringToken(null, "\""));
            AssertNext(new StringToken(null, "\0"));
        }

        [TestMethod]
        public void String()
        {
            initializeLexer("\"prettyHans\"");
            AssertNextString("prettyHans");
        }

        [TestMethod]
        public void FixPlus()
        {
            initializeLexer("+");
            AssertNextFixToken(Tag.Plus);
        }

        [TestMethod]
        public void FixMinus()
        {
            initializeLexer("-");
            AssertNextFixToken(Tag.Minus);
        }

        [TestMethod]
        public void FixTimes()
        {
            initializeLexer("*");
            AssertNextFixToken(Tag.Times);
        }

        [TestMethod]
        public void FixModulo()
        {
            initializeLexer("%");
            AssertNextFixToken(Tag.Modulo);
        }

        [TestMethod]
        public void FixEquals()
        {
            initializeLexer("==");
            AssertNextFixToken(Tag.Equals);
        }

        [TestMethod]
        public void FixAssign()
        {
            initializeLexer("=");
            AssertNextFixToken(Tag.Assign);
        }

        [TestMethod]
        public void FixUnequal()
        {
            initializeLexer("!=");
            AssertNextFixToken(Tag.Unequal);
        }

        [TestMethod]
        public void FixNot()
        {
            initializeLexer("!");
            AssertNextFixToken(Tag.Not);
        }

        [TestMethod]
        public void FixLessEqual()
        {
            initializeLexer("<=");
            AssertNextFixToken(Tag.LessEqual);
        }

        [TestMethod]
        public void FixLess()
        {
            initializeLexer("<");
            AssertNextFixToken(Tag.Less);
        }

        [TestMethod]
        public void FixGreaterEqual()
        {
            initializeLexer(">=");
            AssertNextFixToken(Tag.GreaterEqual);
        }

        [TestMethod]
        public void FixGreater()
        {
            initializeLexer(">");
            AssertNextFixToken(Tag.Greater);
        }

        [TestMethod]
        public void FixAnd()
        {
            initializeLexer("&&");
            AssertNextFixToken(Tag.And);
        }

        [TestMethod]
        public void FixAndIncomplete()
        {
            initializeLexer("&hans");
            AssertNextError(new Location(1, 2));
        }

        [TestMethod]
        public void FixOr()
        {
            initializeLexer("||");
            AssertNextFixToken(Tag.Or);
        }

        [TestMethod]
        public void FixOrIncomplete()
        {
            initializeLexer("|hans");
            AssertNextError(new Location(1, 2));
        }

        [TestMethod]
        public void FixOpenBrace()
        {
            initializeLexer("{");
            AssertNextFixToken(Tag.OpenBrace);
        }

        [TestMethod]
        public void FixCloseBrace()
        {
            initializeLexer("}");
            AssertNextFixToken(Tag.CloseBrace);
        }

        [TestMethod]
        public void FixOpenBracket()
        {
            initializeLexer("[");
            AssertNextFixToken(Tag.OpenBracket);
        }

        [TestMethod]
        public void FixCloseBracket()
        {
            initializeLexer("]");
            AssertNextFixToken(Tag.CloseBracket);
        }

        [TestMethod]
        public void FixOpenParenthesis()
        {
            initializeLexer("(");
            AssertNextFixToken(Tag.OpenParenthesis);
        }

        [TestMethod]
        public void FixCloseParenthesis()
        {
            initializeLexer(")");
            AssertNextFixToken(Tag.CloseParenthesis);
        }

        [TestMethod]
        public void FixColon()
        {
            initializeLexer(":");
            AssertNextFixToken(Tag.Colon);
        }

        [TestMethod]
        public void FixComma()
        {
            initializeLexer(",");
            AssertNextFixToken(Tag.Comma);
        }

        [TestMethod]
        public void FixPeriod()
        {
            initializeLexer(".");
            AssertNextFixToken(Tag.Period);
        }

        [TestMethod]
        public void FixSemicolon()
        {
            initializeLexer(";");
            AssertNextFixToken(Tag.Semicolon);
        }

        [TestMethod]
        public void CharA()
        {
            initializeLexer("'a'");
            AssertNext(new CharacterToken(null, 'a'));
            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CharNewline()
        {
            initializeLexer(@"'\n'");
            AssertNext(new CharacterToken(null, '\n'));
            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CharInvalidEscape()
        {
            initializeLexer(@"'\p'");
            AssertNextError(new Location(1, 1));
            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CharTooMany()
        {
            initializeLexer("'zz';");
            AssertNextError(new Location(1, 1));
            AssertNext(new IdentifierToken(null, "z"));
            AssertNextError(new Location(1, 4));
            AssertNext(new FixToken(null, Tag.End));
        }

        public void CharBackslash()
        {
            initializeLexer(@"'\\'");
            AssertNext(new CharacterToken(null, '\\'));
            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CharUnterminated()
        {
            initializeLexer("'h;");
            AssertNextError(new Location(1, 1));
            AssertNext(new FixToken(null, Tag.Semicolon));
        }

        [TestMethod]
        public void CharTooFew()
        {
            initializeLexer("'';");
            AssertNextError(new Location(1, 1));
            AssertNextError(new Location(1, 2));
            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CombinedAssignment()
        {
            initializeLexer();
            AssertNext(new IdentifierToken(null, "int"));
            AssertNext(new IdentifierToken(null, "i"));
            AssertNext(new FixToken(null, Tag.Semicolon));
            AssertNext(new IdentifierToken(null, "i"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IntegerToken(null, 12));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "string"));
            AssertNext(new IdentifierToken(null, "s"));
            AssertNext(new FixToken(null, Tag.OpenBracket));
            AssertNext(new FixToken(null, Tag.CloseBracket));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "s"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new FixToken(null, Tag.New));
            AssertNext(new IdentifierToken(null, "string"));
            AssertNext(new FixToken(null, Tag.OpenBracket));
            AssertNext(new IntegerToken(null, 17));
            AssertNext(new FixToken(null, Tag.CloseBracket));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "s"));
            AssertNext(new FixToken(null, Tag.OpenBracket));
            AssertNext(new IntegerToken(null, 12));
            AssertNext(new FixToken(null, Tag.CloseBracket));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new StringToken(null, "Hello"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "string"));
            AssertNext(new IdentifierToken(null, "w"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "w"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IdentifierToken(null, "s"));
            AssertNext(new FixToken(null, Tag.OpenBracket));
            AssertNext(new IdentifierToken(null, "i"));
            AssertNext(new FixToken(null, Tag.CloseBracket));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "char"));
            AssertNext(new IdentifierToken(null, "c"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "c"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new CharacterToken(null, 'c'));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new FixToken(null, Tag.End));
        }

        [TestMethod]
        public void CombinedHelloWorld()
        {
            initializeLexer();
            AssertNext(new FixToken(null, Tag.Class));
            AssertNext(new IdentifierToken(null, "Hello"));
            AssertNext(new FixToken(null, Tag.OpenBrace));
            AssertNext(new IdentifierToken(null, "void"));
            AssertNext(new IdentifierToken(null, "Main"));
            AssertNext(new FixToken(null, Tag.OpenParenthesis));
            AssertNext(new FixToken(null, Tag.CloseParenthesis));
            AssertNext(new FixToken(null, Tag.OpenBrace));
            AssertNext(new IdentifierToken(null, "Outputter"));
            AssertNext(new FixToken(new Location(9,12), Tag.Period));
            AssertNext(new IdentifierToken(null, "inscribe"));
            AssertNext(new FixToken(null, Tag.OpenParenthesis));
            AssertNext(new StringToken(null, "Hello World"));
            AssertNext(new FixToken(null, Tag.CloseParenthesis));
            AssertNext(new FixToken(null, Tag.Semicolon));
            AssertNext(new FixToken(null, Tag.CloseBrace));
            AssertNext(new FixToken(null, Tag.CloseBrace));
        }

        [TestMethod]
        public void CombinedEverything()
        {
            //gonna write that sometime.
        }

        public void CombinedOperators()
        {
            initializeLexer();
            AssertNext(new FixToken(null, Tag.Class));
            AssertNext(new IdentifierToken(null, "Ops"));
            AssertNext(new FixToken(null, Tag.OpenBrace));
            AssertNext(new IdentifierToken(null, "void"));
            AssertNext(new IdentifierToken(null, "Main"));
            AssertNext(new FixToken(null, Tag.OpenParenthesis));
            AssertNext(new FixToken(null, Tag.CloseParenthesis));
            AssertNext(new FixToken(null, Tag.OpenBrace));
            AssertNext(new IdentifierToken(null, "int"));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Semicolon));
            AssertNext(new IdentifierToken(null, "int"));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Semicolon));
            AssertNext(new IdentifierToken(null, "bool"));
            AssertNext(new IdentifierToken(null, "x"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IntegerToken(null, 1));
            AssertNext(new FixToken(null, Tag.Plus));
            AssertNext(new IntegerToken(null, 2));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IntegerToken(null, 2));
            AssertNext(new FixToken(null, Tag.Divide));
            AssertNext(new IntegerToken(null, 3));
            AssertNext(new FixToken(null, Tag.Times));
            AssertNext(new IntegerToken(null, 4));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Modulo));
            AssertNext(new FixToken(null, Tag.OpenParenthesis));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Minus));
            AssertNext(new IntegerToken(null, 9));
            AssertNext(new FixToken(null, Tag.CloseParenthesis));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "x"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Less));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.And));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.LessEqual));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "x"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new FixToken(null, Tag.OpenParenthesis));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Greater));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Or));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.GreaterEqual));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.CloseParenthesis));
            AssertNext(new FixToken(null, Tag.And));
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.Equals));
            AssertNext(new IdentifierToken(null, "b"));
            AssertNext(new FixToken(null, Tag.Semicolon));

            AssertNext(new IdentifierToken(null, "x"));
            AssertNext(new FixToken(null, Tag.Assign));
            AssertNext(new IdentifierToken(null, "x"));
            AssertNext(new FixToken(null, Tag.Unequal));
            AssertNext(new FixToken(null, Tag.Not));
            AssertNext(new IdentifierToken(null, "x"));

        }

        [TestMethod]
        public void InvalidCharacters()
        {
            initializeLexer("$a");
            AssertNextError(L);
            AssertNext(new IdentifierToken(null, "a"));
            AssertNext(new FixToken(null, Tag.End));
        }


        private void AssertEnd(int col)
        {
            AssertNext(new FixToken(new Location(1, col), Tag.End));
        }

        private void AssertNextError(Location expected)
        {
            var actual = _lexer.Next();

            Assert.IsInstanceOfType(actual, typeof(ErrorToken));

            Assert.AreEqual(expected, actual.Location);

            Assert.IsTrue(Diagnosis.HasErrors);

            Diagnosis.Clear();
        }

        private void AssertNextString(string content) {
            AssertNext(new StringToken(L, content));
        }

        private void AssertNextIdentifier(string name) {
            AssertNext(new IdentifierToken(L, name));
        }

        private void AssertNextFixToken(Tag t)
        {
            AssertNext(new FixToken(L, t));
        }

        private void AssertNext(Token expected)
        {
            var actual = _lexer.Next();
            if(expected.Location != null)
            {
                Assert.AreEqual(expected.Location, actual.Location);
            }
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

    }
}
