using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Lexer;
using System.IO;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Checker.Visitors;

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
            Assert.IsTrue(Diagnosis.HasErrors);
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
        public void ArrayCreation()
        {
            initializeChecker();
        }

        [TestMethod]
        public void ArrayCreationInvalid()
        {
            initializeChecker();
            AssertDiagnosisContains("");
        }

        [TestMethod]
        public void AssignmentAssignInvalidType()
        {
            initializeChecker(main("int a; a = true;"));
            AssertDiagnosisContains("cannot assign 'bool' to 'int'");
        }

        [TestMethod]
        public void AssignmentArrayLengthInvalid()
        {
            try
            {
                initializeChecker();
            }
            catch (Exception)
            {
                AssertDiagnosisContains("length must not be on the left side");
            }
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
        public void BinaryExpressionBool()
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

        [TestMethod]
        public void MethodReturnStatement()
        {
            initializeChecker();
        }

        [TestMethod]
        public void MethodReturnStatementInvalid()
        {
            try
            {
                initializeChecker();
            }catch(CheckerException)
            {
                AssertDiagnosisContains("");
            }
        }


        public void BinaryExpressionComparisonLiteral()
        {
            initializeChecker(expression("bool", "1234 < 1"));
        }

        [TestMethod]
        public void BinaryExpressionComparisonLiteralUnequalTypes()
        {
            try
            {
                initializeChecker(expression("bool", "1234 < true"));
            }catch(CheckerException)
            {
                AssertDiagnosisContains("Invalid types");
            }
        }

        [TestMethod]
        public void BinaryExpressionComparison()
        {
            initializeChecker(main("int a; int b; bool c; a=123; b=234; c = a != b;"));
        }

        [TestMethod]
        public void BinaryExpressionComparisonUnequalTypes()
        {
            try {
                initializeChecker(main("int a; bool b; bool c; a=123; b=234; c = a != b;"));
            } catch (Exception)
            {
                AssertDiagnosisContains("Invalid types");
            }
        }

        public void WhileConditionInt()
        {
            initializeChecker(main("while(1){};"));
            AssertDiagnosisContains("must be");
        }

        [TestMethod]
        public void WhileConditionBool()
        {
            initializeChecker(main("while(true){};"));
        }

        [TestMethod]
        public void IfConditionInt()
        {
            initializeChecker(main("if(1){};"));
            AssertDiagnosisContains("must be");
        }

        [TestMethod]
        public void ArrayLength()
        {
            initializeChecker(main("int l; int[] arr; arr = new int[10]; l = arr.length;"));
        }

        [TestMethod]
        public void IfConditionBool()
        {
            initializeChecker(main("if(true){};"));
        }

        [TestMethod]
        public void Mega1()
        {
            initializeChecker();
        }
    }
}
