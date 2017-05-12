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
using RappiSharp.VirtualMachine.Runtime;
using RappiSharp.Compiler;
using RappiSharp.VirtualMachine.Error;

namespace _Test
{
    [TestClass]
    public class InterpreterTest : AbstractTest
    {
        protected override string getPathInProject() { return "Generator"; }

        private Interpreter _interpreter;
        private TestConsole _console;
        private bool _didRun;

        private void initializeInterpreter(RappiLexer lexer)
        {
            var generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
            if (Diagnosis.HasErrors)
            {
                Assert.Fail("Error while generating code: " + Diagnosis.Messages);
            }
            _console = new TestConsole();
            _interpreter = new Interpreter(generator.Metadata, _console);
        }

        private void runInterpreter()
        {
            initializeInterpreter(makeLexer());
            Run();
        }

        private void runInterpreter(string code)
        {
            initializeInterpreter(makeLexer(code));
            Run();
        }

        private void runInterpreter(Metadata _metadata)
        {
            _console = new TestConsole();
            _interpreter = new Interpreter(_metadata, _console);
            Run();
        }

        private void initializeInterpreter()
        {
            initializeInterpreter(makeLexer());
        }

        private void initializeInterpreter(string code)
        {
            initializeInterpreter(makeLexer(code));
        }

        private string writeBool(string expression)
        {
            return "if(" + expression + "){ WriteString(\"TRUE\"); } else { WriteString(\"FALSE\"); }";
        }

        public void Run()
        {
            _didRun = true;
            _interpreter.Run();
        }

        [TestInitialize]
        public void Before()
        {
            _didRun = false;
        }

        [TestCleanup]
        public void After()
        {
            if (!_didRun)
            {
                Assert.Fail("Test didn't run!");
            }
        }

        class TestConsole : IConsole, IDisposable
        {

            private Queue<int> _input = new Queue<int>();
            private StringWriter _output = new StringWriter();

            public StringWriter Output
            {
                get { return _output; }
                set { _output = value; }
            }

            public int Read()
            {
                return _input.Dequeue();
            }

            public string ReadLine()
            {
                string s = "";
                int c = _input.Dequeue();
                while(c != '\n')
                {
                    s += (char)c;
                    c = _input.Dequeue();
                }
                return s;
            }

            public void Write(object o)
            {
                _output.Write(o);
            }

            public void Send(char input)
            {
                _input.Enqueue(input);
            }

            public void Send(string input)
            {
                foreach(var c in input.ToCharArray()){
                    _input.Enqueue(c);
                }
                _input.Enqueue('\n');
            }

            public void Dispose()
            {
                _output.Dispose();
            }
        }

        private string program(string content)
        {
            return "class Program{ " + content + "}";

        }

        private string WriteBool(string expression)
        {
            return "if(" + expression + "){ WriteString(\"TRUE\"); } else { WriteString(\"FALSE\"); }";
        }

        [TestMethod]
        public void WriteString()
        {
            runInterpreter(main("WriteString(\"Hello World\");"));
            Assert.AreEqual("Hello World", _console.Output.ToString());
        }

        [TestMethod]
        public void WriteChar()
        {
            runInterpreter(main("WriteChar('\n');"));
            Assert.AreEqual("\n", _console.Output.ToString());
        }

        [TestMethod]
        public void WriteInt()
        {
            runInterpreter(main("WriteInt(12);"));
            Assert.AreEqual("12", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadString()
        {
            initializeInterpreter(main("WriteString(ReadString());"));
            _console.Send("Hello Wurld");
            Run();
            Assert.AreEqual("Hello Wurld", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadChar()
        {
            initializeInterpreter(main("WriteChar(ReadChar());"));
            _console.Send('x');
            Run();
            Assert.AreEqual("x", _console.Output.ToString());
        }

        [TestMethod]
        public void ReadInt()
        {
            initializeInterpreter(main("WriteInt(ReadInt());"));
            _console.Send("9001");
            Run();
            Assert.AreEqual("9001", _console.Output.ToString());
        }

        [TestMethod]
        public void LocalsLoadAndStore()
        {
            runInterpreter(main("int i; i = 12; WriteInt(i);"));
            Assert.AreEqual("12", _console.Output.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidILException))]
        public void InvalidLoadConstant()
        {
            runInterpreter(
                new MetadataBuilder()
                .addMainLocal(-3)
                .addMainInst(OpCode.ldc_i, false)
                .addMainInst(OpCode.stloc, 0)
                .build()
                );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidILException))]
        public void InvalidTopOfStackInBreakFalse()
        {
            runInterpreter(
                new MetadataBuilder()
                .addMainInst(OpCode.ldc_i, 12)
                .addMainInst(OpCode.brfalse, 0)
                .build()
                );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidILException))]
        public void InvalidTypesInAddition()
        {
            runInterpreter(
                new MetadataBuilder()
                .addMainLocal(-3)
                .addMainInst(OpCode.ldc_i, 12)
                .addMainInst(OpCode.ldc_b, false)
                .addMainInst(OpCode.add)
                .addMainInst(OpCode.stloc, 0)
                .build()
                );
        }

        [TestMethod]
        [ExpectedException(typeof(VMException))]
        public void InvalidIndexArrayAccess()
        {
            runInterpreter(
                new MetadataBuilder()
                .addArrayType(-3)
                .addMainLocal(1)
                .addMainLocal(-3)
                .addMainInst(OpCode.ldc_i, 2)
                .addMainInst(OpCode.newarr, 1)
                .addMainInst(OpCode.stloc, 0)
                .addMainInst(OpCode.ldloc, 0)
                .addMainInst(OpCode.ldc_i, 12)
                .addMainInst(OpCode.ldelem)
                .addMainInst(OpCode.stloc, 0)
                .build()
                );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidILException))]
        public void LocalsStoreInvalidType()
        {
            runInterpreter(
                new MetadataBuilder()
                .addMainLocal(-3)
                .addMainInst(OpCode.ldc_b, false)
                .addMainInst(OpCode.stloc, 0)
                .build()
                );
        }

        [TestMethod]
        public void OpAdd()
        {
            runInterpreter(main("WriteInt(1+2);"));
            Assert.AreEqual("3", _console.Output.ToString());
        }

        [TestMethod]
        public void OpSub()
        {
            runInterpreter(main("WriteInt(1-2);"));
            Assert.AreEqual("-1", _console.Output.ToString());
        }

        [TestMethod]
        public void OpMul()
        {
            runInterpreter(main("WriteInt(2*4);"));
            Assert.AreEqual("8", _console.Output.ToString());
        }

        [TestMethod]
        public void OpDiv()
        {
            runInterpreter(main("WriteInt(15/3);"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void OpMod()
        {
            runInterpreter(main("WriteInt(17%5);"));
            Assert.AreEqual("2", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCltTrue()
        {
            runInterpreter(main(WriteBool("1 < 2")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCltFalse()
        {
            runInterpreter(main(WriteBool("2 < 1")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCleTrue()
        {
            runInterpreter(main(WriteBool("1 <= 2")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCleFalse()
        {
            runInterpreter(main(WriteBool("2 <= 1")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCleEqual()
        {
            runInterpreter(main(WriteBool("2 <= 2")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCgtTrue()
        {
            runInterpreter(main(WriteBool("2 > 1")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCgtFalse()
        {
            runInterpreter(main(WriteBool("1 > 2")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCgeTrue()
        {
            runInterpreter(main(WriteBool("2 >= 1")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCgeFalse()
        {
            runInterpreter(main(WriteBool("1 >= 2")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCgeEqual()
        {
            runInterpreter(main(WriteBool("2 >= 2")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCeqTrue()
        {
            runInterpreter(main(WriteBool("1 == 1")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCeqFalse()
        {
            runInterpreter(main(WriteBool("1 == 2")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCneTrue()
        {
            runInterpreter(main(WriteBool("1 != 2")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void OpCneFalse()
        {
            runInterpreter(main(WriteBool("1 != 1")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void IfFalse()
        {
            runInterpreter(main("if(false){ WriteInt(0); } WriteInt(1);"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void IfTrue()
        {
            runInterpreter(main("if(true){ WriteInt(0); } WriteInt(1);"));
            Assert.AreEqual("01", _console.Output.ToString());
        }

        [TestMethod]
        public void IfElseTrue()
        {
            runInterpreter(main($"{WriteBool("true")} WriteInt(0);"));
            Assert.AreEqual("TRUE0", _console.Output.ToString());
        }

        [TestMethod]
        public void IfElseFalse()
        {
            runInterpreter(main($"{WriteBool("false")} WriteInt(0);"));
            Assert.AreEqual("FALSE0", _console.Output.ToString());
        }

        [TestMethod]
        public void WhileSkip()
        {
            runInterpreter(main("while(false){ WriteInt(0); } WriteInt(1);"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void While()
        {
            runInterpreter(main("int i; while(i < 5){ WriteInt(i); i = i+1; }"));
            Assert.AreEqual("01234", _console.Output.ToString());
        }

        [TestMethod]
        public void CallVirtSimple()
        {
            runInterpreter(program("void Main(){ F(); } void F(){ WriteInt(0); }"));
            Assert.AreEqual("0", _console.Output.ToString());
        }

        [TestMethod]
        public void CallVirtArg()
        {
            runInterpreter(program("void Main(){ F(1); } void F(int i){ WriteInt(i); }"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void CallVirtArgs()
        {
            runInterpreter(program("void Main(){ F(1, 'c', \"foo\", true); } void F(int i, char c, string s, bool b){ WriteInt(i); WriteChar(c); WriteString(s); "+WriteBool("b")+"}"));
            Assert.AreEqual("1cfooTRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void Starg()
        {
            runInterpreter(program("void Main(){ F(1); } void F(int i){ WriteInt(i); i = 2; WriteInt(i); }"));
            Assert.AreEqual("12", _console.Output.ToString());
        }

        [TestMethod]
        public void Stloc()
        {
            runInterpreter(program("void Main(){ F(); } void F(){ int i; i=1; WriteInt(i);}"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void ReturnValue()
        {
            runInterpreter(program("void Main(){ WriteInt(F()); } int F(){return 1;}"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void ArrayLength()
        {
            runInterpreter(main("int[] i; i = new int[5]; WriteInt(i.length);"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void ArrayAssignment()
        {
            runInterpreter(main("int[] i; i = new int[5]; i[3] = 7; WriteInt(i[3]);"));
            Assert.AreEqual("7", _console.Output.ToString());
        }

        [TestMethod]
        public void CallVirtWithArray()
        {
            runInterpreter(program("void Main(){ int[] i; i = new int[5]; F(i); WriteInt(i[0]);} void F(int[] i){ i[0] = 7; }"));
            Assert.AreEqual("7", _console.Output.ToString());
        }

        [TestMethod]
        public void CallVirtReturnArray()
        {
            runInterpreter(program("void Main(){ int[] i; i = F(); WriteInt(i[0]);} int[] F(){ int[] x; x = new int[5]; x[0] = 7; return x;}"));
            Assert.AreEqual("7", _console.Output.ToString());
        }

        [TestMethod]
        public void Dijkstra()
        {
            runInterpreter();
            Assert.AreEqual("0 0\n1 4\n2 12\n3 19\n4 21\n5 11\n6 9\n7 8\n8 14\n", _console.Output.ToString());
        }

        [TestMethod]
        public void AVL()
        {
            runInterpreter();
            Assert.AreEqual("30 20 10 25 40 50 ", _console.Output.ToString());
        }

        [TestMethod]
        public void Greeter()
        {
            runInterpreter();
            Assert.AreEqual("Hello thou beautiful World", _console.Output.ToString());
        }

        [TestMethod]
        public void ObjectNew()
        {
            runInterpreter(main("Program p; p = new Program();"));
            //Assert.AreEqual("7", _console.Output.ToString());
        }

        [TestMethod]
        public void ObjectField()
        {
            runInterpreter(program("int i; void Main(){ i = 5; WriteInt(i); }"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void CallMemberArg()
        {
            runInterpreter();
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void ClassMemberAccess()
        {
            runInterpreter();
            Assert.AreEqual("0", _console.Output.ToString());
        }

        [TestMethod]
        public void CallClassMemberMethod()
        {
            runInterpreter();
            Assert.AreEqual("Lul", _console.Output.ToString());
        }

        [TestMethod]
        public void DynamicDispatch()
        {
            runInterpreter();
            Assert.AreEqual("LulHahaROFL", _console.Output.ToString());
        }

        [TestMethod]
        public void Polymorphism()
        {
            runInterpreter();
        }

        [TestMethod]
        public void PolymorphismComplex()
        {
            runInterpreter();
        }

        [TestMethod]
        public void BinaryExprRefEq()
        {
            runInterpreter();
            Assert.AreEqual("Cookies!", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExprRefNEq()
        {
            runInterpreter();
            Assert.AreEqual("never!", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExprRefEqNull()
        {
            runInterpreter();
            Assert.AreEqual("never!", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExprRefThis()
        {
            runInterpreter();
            Assert.AreEqual("give!", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExprRefNEqNull()
        {
            runInterpreter();
            Assert.AreEqual("never!", _console.Output.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(VMException))]
        public void ClassCastInvalid()
        {
            runInterpreter();
        }

        [TestMethod]
        public void ClassCastNull()
        {
            runInterpreter();
        }

        [TestMethod]
        public void ClassCast()
        {
            runInterpreter();
            Assert.AreEqual("b.foo", _console.Output.ToString());
        }

        [TestMethod]
        public void NestedArrayAccess()
        {
            runInterpreter();
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void IsSameClass()
        {
            //runInterpreter("class P { void Main(A a; a=new a(); "+WriteBool("a is A")+") } class A{}");
            runInterpreter("class P { void Main(){A a; a=new A(); "+WriteBool("a is A")+" }} class A{}");
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void IsNull()
        {
            runInterpreter("class P { void Main(){A a; "+WriteBool("a is A")+" }} class A{}");
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void IsOtherClass()
        {
            runInterpreter("class P { void Main(){A a; a = new A1(); "+WriteBool("a is A2")+" }} class A{} class A1:A{} class A2:A{}");
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void IsSubClass()
        {
            runInterpreter("class P { void Main(){A a; a=new B(); "+WriteBool("a is B")+" }} class A{} class B:A{}");
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void AssignNullArray()
        {
            runInterpreter(main("Program[] p; p = null;" +WriteBool("p == null")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void AssignNullObject()
        {
            runInterpreter(main("Program p; p = null;" +WriteBool("p == null")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void Playground()
        {
            runInterpreter();
            Run();
        }

        [TestMethod]
        public void CallMemberMethodComplex()
        {
            runInterpreter();
            Assert.AreEqual("1!yay", _console.Output.ToString());
        }

        [TestMethod]
        public void CallMemberReturn()
        {
            runInterpreter();
            Run();
        }

        [TestMethod]
        public void ArrayCreation()
        {
            runInterpreter();
            Run();
        }

        [TestMethod]
        public void BinaryExpressionOr1()
        {
            runInterpreter();
            Assert.AreEqual("True", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExpressionOr2()
        {
            runInterpreter();
            Assert.AreEqual("AAFalse", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExpressionAnd1()
        {
            runInterpreter();
            Assert.AreEqual("True", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExpressionAnd2()
        {
            runInterpreter();
            Assert.AreEqual("False", _console.Output.ToString());
        }

        [TestMethod]
        public void BinaryExpressionAnd3()
        {
            runInterpreter();
            Assert.AreEqual("AFalse", _console.Output.ToString());
        }

        [TestMethod]
        public void BigMath()
        {
            runInterpreter();
            Assert.AreEqual("0-3", _console.Output.ToString());
        }

        [TestMethod]
        public void ArrayNullCompareTrue()
        {
            runInterpreter(main("int[] a; " + writeBool("a == null")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void ArrayNullCompareFalse()
        {
            runInterpreter(main("int[] a; a = new int[10]; " + writeBool("a == null")));
            Assert.AreEqual("FALSE", _console.Output.ToString());
        }

        [TestMethod]
        public void ArrayAssignNull()
        {
            runInterpreter(main("int[] a; a = new int[10]; a = null; " + writeBool("a == null")));
            Assert.AreEqual("TRUE", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessThisImplicitPrimitive()
        {
            runInterpreter(program("int i; void Main(){ i = 5; WriteInt(i); }"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessThisPrimitive()
        {
            runInterpreter(program("int i; void Main(){ this.i = 5; WriteInt(this.i); }"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void DesignatorComplex()
        {
            runInterpreter();
            Assert.AreEqual("Apple" + "OM NOM NOM, TASTY Apple" + "OM NOM NOM, TASTY Apple3", _console.Output.ToString());
        }

        [TestMethod]
        public void ObjectCreation()
        {
            runInterpreter(main("Program p; p = new Program();"));
            Run();
        }

        [TestMethod]
        public void ClassMemberAccessArray()
        {
            runInterpreter();
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void ClassMemberAccessMemberArray()
        {
            runInterpreter();
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessThisImplicitMethod()
        {
            runInterpreter(program("void Foo(){ WriteInt(5); } void Main(){ Foo(); }"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessThisMethod()
        {
            runInterpreter(program("void Foo(){ WriteInt(5); } void Main(){ this.Foo(); }"));
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessMemberMethod()
        {
            runInterpreter("class Program{ void Main(){ Foo f; f = new Foo(); f.Foo(); } } class Foo{ void Foo(){WriteInt(5); } }");
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void MemberAccessMemberArrayMethod()
        {
            runInterpreter("class Program{ void Main(){ Foo[] fs; fs = new Foo[10]; fs[0] = new Foo(); fs[0].Foo(); } } class Foo{ void Foo(){WriteInt(5); } }");
            Assert.AreEqual("5", _console.Output.ToString());
        }

        [TestMethod]
        public void ReturnStatement()
        {
            runInterpreter(main("WriteInt(1); return; WriteInt(2);"));
            Assert.AreEqual("1", _console.Output.ToString());
        }

        [TestMethod]
        public void IsOperator()
        {
            runInterpreter();
            Assert.AreEqual("a is B", _console.Output.ToString());
        }
        
        [TestMethod]
        public void WhileStatement()
        {
            runInterpreter(main("int i; while(i<5){i=i+1; WriteInt(i);};"));
            Assert.AreEqual("12345", _console.Output.ToString());
        }
        
        [TestMethod]
        public void MergeSort()
        {
            runInterpreter();
            Assert.AreEqual("0,1,2,3,4,5,6,7,8,9", _console.Output.ToString());
        }
        
        [TestMethod]
        public void QuickSort()
        {
            runInterpreter();
            Assert.AreEqual("0,1,2,3,4,5,6,7,8,9", _console.Output.ToString());
        }

        [TestMethod]
        public void MaxFlow()
        {
            runInterpreter();
            Assert.AreEqual("23", _console.Output.ToString());
        }
        
        [TestMethod]
        public void DefaultInitIntArr()
        {
            runInterpreter(main("int[] a; a = new int[1]; WriteInt(a[0]);"));
            Assert.AreEqual("0", _console.Output.ToString());
        }
        
        [TestMethod]
        public void ArrayList()
        {
            runInterpreter();
            Assert.AreEqual("Peter Muster\n3\nHans Meier\n2", _console.Output.ToString());
        }
        
        [TestMethod]
        public void PolymorphismFields()
        {
            runInterpreter();
            Assert.AreEqual("1942", _console.Output.ToString());
        }
        
        [TestMethod]
        public void NullDerefObject()
        {
            try
            {
                runInterpreter(main("Program p; p.Main();"));
                Assert.Fail("Exception expected");
            } catch (VMException e)
            {
                Assert.IsTrue(e.Message.Contains("null"));
            }
        }

        [TestMethod]
        public void NullDerefArray()
        {
            try
            {
                runInterpreter(main("int[] arr; WriteInt(arr.length);"));
                Assert.Fail("Exception expected");
            } catch (VMException e)
            {
                Assert.IsTrue(e.Message.Contains("null"));
            }
        }
        
    }
}