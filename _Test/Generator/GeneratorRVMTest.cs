using _Test.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler.Generator;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;
using System;
using System.Collections.Generic;
using static RappiSharp.IL.OpCode;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;
using System.Text;
using RappiSharp.Compiler;

namespace _Test
{
    [TestClass]
    public class GeneratorRVMTest : AbstractTest
    {
        protected override string getPathInProject() { return "Generator"; }

        private RappiGenerator _generator;

        private ILIterator initializeGenerator(RappiLexer lexer)
        {
            var checker = new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable;
            Assert.IsFalse(Diagnosis.HasErrors, "Errors before generator: " + Diagnosis.Messages);
            _generator = new RappiGenerator(checker);
            return new ILIterator(_generator.Metadata.Methods[_generator.Metadata.MainMethod].Code);
        }

        private ILIterator initializeGenerator()
        {
            return initializeGenerator(makeLexer());
        }

        private ILIterator initializeGenerator(string code)
        {
            return initializeGenerator(makeLexer(code));
        }

        private string expression(string type, string expr)
        {
            return main($"{type} a; a = {expr};");
        }

        private string program(string content)
        {
            return "class Program{ "+content+ "}";

        }

        private string writeBool(string expression)
        {
            return "if(" + expression + "){ WriteString(\"TRUE\"); } else { WriteString(\"FALSE\"); }";
        }

        private ILIterator method(string name)
        {
            foreach(var method in _generator.Metadata.Methods)
            {
                if(method.Identifier == name)
                {
                    return new ILIterator(method.Code);
                }
            }
            Assert.Fail($"Method {name} not found");
            return null;
        }

        public string Run(string expectedOutput = null)
        {
            //Run it 
            var serializer = new XmlSerializer(typeof(Metadata));
            using (var stream = new FileStream("test.il", FileMode.Create, FileAccess.Write)) {
                serializer.Serialize(stream, _generator.Metadata);
            }
            var proc = new Process();
            var stdout = "";
            var output = new StringBuilder();
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.OutputDataReceived += (sender, args) => stdout += output.Append(args.Data);
            proc.ErrorDataReceived += (sender, args) => stdout += output.Append(args.Data);
            proc.StartInfo.FileName = "../../Generator/rvm.exe";
            proc.StartInfo.Arguments = "test.il";
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit(2000);
            var str = output.ToString();
            if(str.Contains("VM ERROR"))
            {
                Assert.Fail("Execution error: " + str);
            }
            if(expectedOutput != null)
            {
                Assert.IsTrue(str.ToLower().Contains(expectedOutput.ToLower()), $"Expected to find '{expectedOutput}' in: '{str}'");
            }
            return output.ToString();
        }

        [TestCleanup]
        public override void tearDown()
        {
            base.tearDown();

        }


        [TestMethod]
        public void Playground()
        {
            initializeGenerator();
            Run();
        }

        [TestMethod]
        public void CallMemberMethodComplex()
        {
            initializeGenerator();
            Run("1!yay");
        }

        [TestMethod]
        public void CallMemberArg()
        {
            initializeGenerator();
            Run("1");
        }

        [TestMethod]
        public void CallMemberReturn()
        {
            initializeGenerator();
            Run();
        }

        [TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator();
            Run();
        }

        [TestMethod]
        public void BinaryExpressionOr1()
        {
            initializeGenerator();
            Run("True");
        }

        [TestMethod]
        public void BinaryExpressionOr2()
        {
            initializeGenerator();
            Run("AAFalse");
        }

        [TestMethod]
        public void BinaryExpressionAnd1()
        {
            initializeGenerator();
            Run("True");
        }

        [TestMethod]
        public void BinaryExpressionAnd2()
        {
            initializeGenerator();
            Run("False");
        }

        [TestMethod]
        public void BinaryExpressionAnd3()
        {
            initializeGenerator();
            Run("AFalse");
        }

        [TestMethod]
        public void BigMath()
        {
            initializeGenerator();
            Run("0-3");
        }

        [TestMethod]
        public void ClassMemberAccess()
        {
            initializeGenerator();
            Run("0");
        }

        [TestMethod]
        public void ArrayLength()
        {
            initializeGenerator(main("int[] a; a = new int[10]; WriteInt(a.length);"));
            Run("10");
        }

        [TestMethod]
        public void ArrayAssignment()
        {
            initializeGenerator(main("int[] a; a = new int[10]; a[0] = 5; WriteInt(a[0]);"));
            Run("5");
        }

        [TestMethod]
        public void ArrayNullCompareTrue()
        {
            initializeGenerator(main("int[] a; " + writeBool("a == null")));
            Run("TRUE");
        }

        [TestMethod]
        public void ArrayNullCompareFalse()
        {
            initializeGenerator(main("int[] a; a = new int[10]; " + writeBool("a == null")));
            Run("FALSE");
        }

        [TestMethod]
        public void ArrayAssignNull()
        {
            initializeGenerator(main("int[] a; a = new int[10]; a = null; " + writeBool("a == null")));
            Run("TRUE");
        }

        [TestMethod]
        public void MemberAccessThisImplicitPrimitive()
        {
            initializeGenerator(program("int i; void Main(){ i = 5; WriteInt(i); }"));
            Run("5");
        }

        [TestMethod]
        public void MemberAccessThisPrimitive()
        {
            initializeGenerator(program("int i; void Main(){ this.i = 5; WriteInt(this.i); }"));
            Run("5");
        }

        [TestMethod]
        public void DesignatorComplex()
        {
            initializeGenerator();
            Run("Apple" + "OM NOM NOM, TASTY Apple" + "OM NOM NOM, TASTY Apple3");
        }

        [TestMethod]
        public void ObjectCreation()
        {
            initializeGenerator(main("Program p; p = new Program();"));
            Run();
        }

        [TestMethod]
        public void NestedArrayAccess()
        {
            initializeGenerator();
            Run("1");
        }

        [TestMethod]
        public void ClassMemberAccessArray()
        {
            initializeGenerator();
            Run("5");
        }

        [TestMethod]
        public void ClassMemberAccessMemberArray()
        {
            initializeGenerator();
            Run("5");
        }

        [TestMethod]
        public void ClassCast()
        {
            initializeGenerator();
            Run("b.foo");
        }

        [TestMethod]
        public void MemberAccessThisImplicitMethod()
        {
            initializeGenerator(program("void Foo(){ WriteInt(5); } void Main(){ Foo(); }"));
            Run("5");
        }

        [TestMethod]
        public void MemberAccessThisMethod()
        {
            initializeGenerator(program("void Foo(){ WriteInt(5); } void Main(){ this.Foo(); }"));
            Run("5");
        }

        [TestMethod]
        public void MemberAccessMemberMethod()
        {
            initializeGenerator("class Program{ void Main(){ Foo f; f = new Foo(); f.Foo(); } } class Foo{ void Foo(){WriteInt(5); } }");
            Run("5");
        }

        [TestMethod]
        public void MemberAccessMemberArrayMethod()
        {
            initializeGenerator("class Program{ void Main(){ Foo[] fs; fs = new Foo[10]; fs[0] = new Foo(); fs[0].Foo(); } } class Foo{ void Foo(){WriteInt(5); } }");
            Run("5");
        }

        [TestMethod]
        public void ReturnStatement()
        {
            initializeGenerator(main("WriteInt(1); return; WriteInt(2);"));
            Run("1");
        }

        [TestMethod]
        public void IsOperator()
        {
            initializeGenerator();
            Run("a is B");
        }
        
        [TestMethod]
        public void WhileStatement()
        {
            initializeGenerator(main("int i; while(i<5){i=i+1; WriteInt(i);};"));
            Run("12345");
        }
    }
}