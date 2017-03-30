﻿using System;
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

        public TestContext TestContext { get; set; }

        private void initializeChecker()
        {
            var lexer = new RappiLexer(File.OpenText("../../Checker/" + TestContext.TestName + ".txt"));
            var parser = new RappiParser(lexer);
            _checker = new RappiChecker(parser.ParseProgram());
        }

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

        private void AssertDiagnosisContains(string msg)
        {
            Assert.IsTrue(Diagnosis.Messages.ToLower().Contains(msg.ToLower()), $"Expected diagnosis to contain '${msg}', but was '${Diagnosis.Messages}'");
            Diagnosis.Clear();
        }

        private string main(string content)
        {
            return "class Prorahm{ void Main(){" + content + "}}";
        }

        private string expression(string type, string expr)
        {
            return main($"{type} a; a = {expr};");
        }

        [TestMethod]
        public void AssignmentAssignInvalidType()
        {
            initializeChecker(main("int a; a = true;"));
            AssertDiagnosisContains("cannot assign bool to int");
        }

        [TestMethod]
        public void AssignmentUnaryNot()
        {
            initializeChecker(expression("bool", "!true;"));
        }

        [TestMethod]
        public void BinaryExpressionInt()
        {
            initializeChecker(expression("int", "1 * 4"));
        }

        [TestMethod]
        public void AssignmentMaxIntValue()
        {
            initializeChecker(expression("int", "" + int.MaxValue));
        }

        [TestMethod]
        public void AssignmentMaxIntValueOverflow()
        {
            initializeChecker(expression("int", "2147483648"));
            AssertDiagnosisContains("maxvalue");
        }

        [TestMethod]
        public void BinaryExpressionMaxValue()
        {
            initializeChecker(expression("int", $"({int.MaxValue} + 1234)"));
        }

        [TestMethod]
        public void BinaryExpressionMaxValueOverflow()
        {
            initializeChecker(expression("int", $"({int.MaxValue+1L} + 1234)"));
            AssertDiagnosisContains("maxvalue");
        }

        [TestMethod]
        public void AssignmentBinaryExpressionBool()
        {
            initializeChecker(expression("bool", "true && true"));
        }

        [TestMethod]
        public void MethodCallNoArgs()
        {
            initializeChecker();
        }

        [TestMethod]
        public void MethodCallManyArgs()
        {
            initializeChecker();
        }

        [TestMethod]
        public void MethodCallArgTypeMismatch()
        {
            initializeChecker();
            AssertDiagnosisContains("cannot assign");
        }

        [TestMethod]
        public void MethodCallArgCountMismatch()
        {
            initializeChecker();
            AssertDiagnosisContains("invalid argument count");
        }
    }
}
