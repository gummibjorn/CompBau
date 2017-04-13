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
            initializeChecker(makeLexer());
        }

        private void initializeChecker(string code)
        {
            initializeChecker(makeLexer(code));
        }

        private void initializeChecker(RappiLexer lexer)
        {
            var parser = new RappiParser(lexer).ParseProgram();
            Assert.IsFalse(Diagnosis.HasErrors, Diagnosis.Messages);
            _checker = new RappiChecker(parser);
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
        public void AssignmentAssignNullToPrimitive()
        {
            initializeChecker(main("int a; a = null;"));
            AssertDiagnosisContains("cannot assign '@NULL' to 'int'");
        }


        [TestMethod]
        public void AssignmentAssignBoolToInt()
        {
            initializeChecker(main("int a; a = true;"));
            AssertDiagnosisContains("cannot assign");
        }

        [TestMethod]
        public void AssignmentAssignNullToArray()
        {
            initializeChecker(main("int[] a; a = null;"));
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
        public void ComparisonBaseSub()
        {
            initializeChecker();
        }

        [TestMethod]
        public void ComparisonInvalidTypes()
        {
            initializeChecker();
            AssertDiagnosisContains("wrong type");
        }

        [TestMethod]
        public void ComparisonInvalidArrayTypes()
        {
            initializeChecker();
            AssertDiagnosisContains("wrong type");
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
        public void MethodCallStatement()
        {
            initializeChecker();
        }

        [TestMethod]
        public void MethodCallStatementNonVoid()
        {
            initializeChecker();
            AssertDiagnosisContains("must return void");
        }

        [TestMethod]
        public void MethodReturnStatement()
        {
            initializeChecker();
        }

        [TestMethod]
        public void MethodReturnStatementInvalid()
        {
            initializeChecker();
            AssertDiagnosisContains("");
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
        public void BinaryExpressionComparisonArrayAccessInvalidExpr()
        {
            initializeChecker(main("int[] a; int b; bool c; a = new int[12]; b=2; c = b == a[\"string\"];"));
            AssertDiagnosisContains("must be of type int");
        }

        [TestMethod]
        public void BinaryExpressionComparisonPrimitiveTypeNull()
        {
            initializeChecker(main("int a; bool c; c = a == null;"));
            AssertDiagnosisContains("Wrong type in comparison");
        }

        [TestMethod]
        public void BinaryExpressionComparisonLiteralUnequalTypes()
        {
            initializeChecker(expression("bool", "1234 < true"));
            AssertDiagnosisContains("Wrong type in binary");
        }

        [TestMethod]
        public void BinaryExpressionComparison()
        {
            initializeChecker(main("int a; int b; bool c; a=123; b=234; c = a != b;"));
        }

        [TestMethod]
        public void BinaryExpressionComparisonUnequalTypes()
        {
            initializeChecker(main("int a; bool b; bool c; a=123; b=234; c = a != b;"));
            AssertDiagnosisContains("Cannot assign");
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
        public void ElementAccessInvalidExpression()
        {
            initializeChecker(main("int[] a; int b; a = new int[10]; b = a[true];"));
            AssertDiagnosisContains("");
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
        public void Mega2()
        {
            initializeChecker();
        }

        [TestMethod]
        public void Playground()
        {
            initializeChecker();
            Console.Write("BREAK HERE");
        }

        [TestMethod]
        public void BuiltinWriteInt()
        {
            initializeChecker(main("WriteInt(0);"));
        }

        [TestMethod]
        public void BuiltinWriteChar()
        {
            initializeChecker(main("WriteChar('!');"));
        }

        [TestMethod]
        public void BuiltinWriteString()
        {
            initializeChecker(main("WriteString(\"HI\");"));
        }

        [TestMethod]
        public void BuiltinHalt()
        {
            initializeChecker(main("Halt(\"BYE\");"));
        }

        [TestMethod]
        public void BuiltinReadInt()
        {
            initializeChecker(expression("int", "ReadInt()"));
        }

        [TestMethod]
        public void BuiltinReadChar()
        {
            initializeChecker(expression("char", "ReadChar()"));
        }

        [TestMethod]
        public void BuiltinReadString()
        {
            initializeChecker(expression("string", "ReadString()"));
        }

        [TestMethod]
        public void ArrayAsParam()
        {
            initializeChecker(@"class Test{void Main() { int[] i; i = new int[10]; Test(i); } void Test(int[] i ) { } }");
        }


        [TestMethod]
        public void manyBinaryExpressions()
        {
            initializeChecker(@"class Test{void Main() { int a; a = 1 + 2 * 3 % !a / b(100);  } int b(int a) { if(a >= a) {return 2*a;} else { return -1;} }}");
            AssertDiagnosisContains("");
        }

        [TestMethod]
        public void MissingReturn()
        {
            initializeChecker(@"class Test{void Main() { } int a1() { if(1==1) {  } else {   } }  }");
        }


        [TestMethod]
        public void returnInVoidMethod()
        {
            initializeChecker(@"class Test{void Main() { return 1+2; }  }");
            AssertDiagnosisContains("");
        }

        [TestMethod]
        public void ReturnEmpty()
        {
            initializeChecker(@"class Test{void Main() { return ; }  }");
        }

        [TestMethod]
        public void AssignReferenceType()
        {
            initializeChecker("class Base{ void Main(){ Base b; b = new Base(); }}");
        }
        
        [TestMethod]
        public void AssignReferenceSubtype()
        {
            initializeChecker("class Base{ void Main(){ Base b; b = new Sub(); }} class Sub : Base {}");
        }

        [TestMethod]
        public void AssignReferenceArrayType()
        {
            initializeChecker("class Base{ void Main(){ Base[] b; b = new Base[1]; b[0] = new Base(); }}");
        }
        
        [TestMethod]
        public void AssignReferenceArraySubtype()
        {
            initializeChecker("class Base{ void Main(){ Base[] b; b = new Base[1]; b[0] = new Sub(); }} class Sub : Base {}");
        }

        [TestMethod]
        public void AssignReferenceSubtypeWrongWay()
        {
            initializeChecker("class Base{ void Main(){ Sub s; s = new Base(); }} class Sub : Base {}");
            AssertDiagnosisContains("cannot assign");
        }

        [TestMethod]
        public void InexistentClassInheritance()
        {
            initializeChecker("class Sub:Base{ void Main(){} }");
            AssertDiagnosisContains("undeclared type");
        }

        [TestMethod]
        public void InexistentClassInstantiation()
        {
            initializeChecker(main("Foo f; f = new Foo();"));
            AssertDiagnosisContains("undeclared type");
        }

        [TestMethod]
        public void IsSameClass()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; b = new Base(); a = b is Base;} }");
        }

        [TestMethod]
        public void IsSameClassInline()
        {
            initializeChecker("class Base{ void Main(){ bool a; a = (new Base() is Base);} }");
        }

        [TestMethod]
        public void IsRightPrimitive()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; a = (b is bool);} }");
            AssertDiagnosisContains("undeclared class");
        }

        [TestMethod]
        public void IsLeftPrimitive()
        {
            initializeChecker("class Base{ void Main(){ bool a; a = (a is Base);} }");
            AssertDiagnosisContains("must be a reference");
        }

        [TestMethod]
        public void IsRightFunky()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; int[] c; a = (b is c[12]);} }");
            AssertDiagnosisContains("must be a class name");
        }

        [TestMethod]
        public void IsSubtype()
        {
            initializeChecker("class Base{ void Main(){ bool a; Sub b; a = (b is Base);} } class Sub:Base{}");
        }

        [TestMethod]
        public void IsSupertype()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; a = (b is Sub);} } class Sub:Base{}");
            AssertDiagnosisContains("can never be");
        }

        [TestMethod]
        public void IsOthertype()
        {
            initializeChecker("class Base{ void Main(){ bool a; Peter p; a = (p is Base);} } class Peter{}");
            AssertDiagnosisContains("can never be");
        }

        [TestMethod, Ignore]
        public void IsLeftPrimitiveArray()
        {
            initializeChecker("class Base{ void Main(){ bool a; int[] b; a = (b is Base);} }");
        }

        [TestMethod, Ignore]
        public void IsRightPrimitiveArray()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; a = (b is int[]);} }");
        }

        [TestMethod, Ignore]
        public void IsReftypeArray()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b[]; a = (b is Base[]);} }");
        }

        [TestMethod, Ignore]
        public void IsReftypeArrayWrongLeft()
        {
            initializeChecker("class Base{ void Main(){ bool a; Base b; a = (b is Base[]);} }");
        }

        [TestMethod]
        public void InexistentMethod()
        {
            initializeChecker(main("Foo();"));
            AssertDiagnosisContains("undeclared method");
        }

        [TestMethod]
        public void CastDown()
        {
            initializeChecker("class Base{ void Main(){ Base b; Sub s; b = new Sub(); s = (Sub)b; } } class Sub:Base{}");
        }

        [TestMethod]
        public void CastPrimitive()
        {
            initializeChecker("class Base{ void Main(){ Base b; int i; b = new Base(); i = (int)b; } }");
            AssertDiagnosisContains("cannot cast");
        }

        [TestMethod]
        public void NewInexistent()
        {
            initializeChecker("class Base{ void Main(){ Base b; b = new Sub(); } }");
            AssertDiagnosisContains("undeclared type");
        }

        [TestMethod]
        public void DesignatorComplex()
        {
            initializeChecker();
            Console.Write("BREAK");
        }

        [TestMethod]
        public void MemberAccessThisImplicitPrimitive()
        {
            initializeChecker("class Program{ int i; void Main(){ i = 5; WriteInt(i); } }");
        }

        [TestMethod]
        public void MemberAccessThisPrimitive()
        {
            initializeChecker("class Program{ int i; void Main(){ this.i = 5; WriteInt(this.i); } }");
        }
    }
}
