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
    public class CheckerTest : AbstractTest
    {
        protected override string getPathInProject() { return "Checker"; }

        private RappiChecker _checker;

        private void initializeChecker()
        {
            _checker = new RappiChecker(new RappiParser(makeLexer()).ParseProgram());
        }

        private void initializeChecker(string code)
        {
            _checker = new RappiChecker(new RappiParser(makeLexer(code)).ParseProgram());
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

        [TestMethod]
        public void BinaryExpressionComparisonLiteral()
        {
            initializeChecker(expression("bool", "1234 < 1"));
        }

        [TestMethod]
        public void BinaryExpressionComparisonReferenceTypeNull()
        {
            initializeChecker(main("int[] a; bool c; c = a == null;"));
        }

        [TestMethod]
        public void BinaryExpressionComparisonPrimitiveTypeNull()
        {
            try
            {
                initializeChecker(main("int a; bool c; c = a == null;"));
            }
            catch (CheckerException)
            {
                AssertDiagnosisContains("Wrong type in comparison");
            }
        }

        [TestMethod]
        public void BinaryExpressionComparisonLiteralUnequalTypes()
        {
            try
            {
                initializeChecker(expression("bool", "1234 < true"));
            }catch(CheckerException)
            {
                AssertDiagnosisContains("Wrong type in binary");
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
            } catch (CheckerException)
            {
                AssertDiagnosisContains("Cannot assign");
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
        public void ElementAccess()
        {
            initializeChecker(main("int l; int[][] arr; int[] sarr; arr = new int[10][]; sarr = new int[1]; arr[0] = sarr; l = arr[0][0];"));
        }

        [TestMethod]
        public void IfConditionBool()
        {
            initializeChecker(main("if(true){};"));
        }

        [TestMethod]
        public void AssignLengthOfObject()
        {
            initializeChecker();
        }

        [TestMethod]
        public void AssignLengthOfArray()
        {
            initializeChecker();
            AssertDiagnosisContains("must not be on the left side");
        }

        [TestMethod]
        public void AssignmentTypeCast()
        {
            initializeChecker();
        }

        [TestMethod]
        public void AssignmentTypeCastInvalid()
        {
            initializeChecker();
            AssertDiagnosisContains("cannot assign");
        }

        [TestMethod]
        public void Mega1()
        {
            initializeChecker();
        }

        [TestMethod]
        public void Playground()
        {
            initializeChecker();
            Console.Write("BREAK HERE");
        }
    }
}
