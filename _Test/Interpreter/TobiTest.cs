using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler.Generator;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using RappiSharp.VirtualMachine;
using RappiSharp.VirtualMachine.Error;
using RappiSharp.VirtualMachine.Runtime;
using _Test.Generator;

namespace _Test
{
    [TestClass]
    public class TobiTest
    {
        [TestInitialize]
        public void Init()
        {
            Diagnosis.Clear();
        }

        public string RunForOutput(string code)
        {
            return RunForOutput(code, new List<string>());
        }

        public string RunForOutput(string code, List<string> input)
        {
            var lexer = new RappiLexer(new StringReader(code));
            var parser = new RappiParser(lexer);
            var checker = new RappiChecker(parser.ParseProgram());
            var generator = new RappiGenerator(checker.SymbolTable);

            var output = ProcessLauncher.RunVm(generator, input);
            var interpreter = new Interpreter(generator.Metadata);
            

            string inp = "";
            foreach (var tex in input)
            {
                inp += tex + "\r\n";
            }

            Console.WriteLine(inp);

            var consoleOut = Console.Out;
            var consoleIn = Console.In;
            var reader = new StringReader(inp);
            var writer = new StringWriter();
            Console.SetIn(reader);
            Console.SetOut(writer);

            interpreter.Run();
            writer.Flush();

            var interpreterOut = writer.GetStringBuilder().ToString();

            Assert.AreEqual(output, interpreterOut);

            Console.SetIn(consoleIn);
            Console.SetOut(consoleOut);

            reader.Close();
            writer.Close();

            return interpreterOut;
        }

        [TestMethod]
        public void ReadLine()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i; 
    string s;
    
    i = ReadInt();
    WriteInt(i);
    
    s = ReadString();
    
    WriteString(""|"");
    WriteString(s);
    
}}
", new List<string>() {"12", "fuu"});

            Assert.AreEqual("12|fuu", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod, Ignore]
        public void ReadLineChar()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    char c;

    c = ReadChar();
    WriteChar(c);

    c = ReadChar();
    WriteChar(c);

    c = ReadChar();
    WriteChar(c);
}}
", new List<string>() {"cde"});

            Assert.AreEqual("cde", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ReadLineString()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    string i;
    i = ReadString();
    WriteString(i);
    
    WriteString(""|"");
    i = ReadString();
    WriteString(i);

    WriteString(""|"");
    i = ReadString();
    WriteString(i);
}}
", new List<string>() {"12asdf", "fuuuuu", "bii"});

            Assert.AreEqual("12asdf|fuuuuu|bii", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestWriteLine()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    WriteString(""TEST"");
}}
");

            Assert.AreEqual("TEST", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignInt()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 1;
    WriteInt(i);
}}
");

            Assert.AreEqual("1", output);
        }

        [TestMethod, Ignore]
        [ExpectedException(typeof(VMException))]
        public void TestHalt()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    WriteString(""AAAA"");
    Halt(""TEST"");
    WriteString(""AAAA"");
}}
");

            Assert.AreNotEqual("AAAAAAAA", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignIntAdd()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 + 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("4", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignIntMult()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 * 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("4", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestAssignIntDiv()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 / 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignIntMinus()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 - 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("0", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestAssignIntMod()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 % 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("0", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignBool()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    bool i;
    i = true;
    WriteInt(100);
}}
");

            Assert.AreEqual("100", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestAssignIf()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    if (true) {
        i = 100;
    }
    else
    {
        i = 10;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("100", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignIfElseBranch()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    if (false) {
        i = 100;
    }
    else
    {
        i = 10;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignWhile()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    bool b; int i;
    b = true; i = 1;
    while (b) {
        i = 10;
        b = false;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestAssignAssign()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int a; int b; int c;
    
    a = 1;
    b = a;
    c = b;
    

    WriteInt(a+b+c);
}}
");

            Assert.AreEqual("3", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestCallMethodWithoutParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(){
    WriteInt(1);
}
void Main(){
    test();
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestCallMethodWithParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(int i){
    WriteInt(i);
}
void Main(){
    test(1);
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestCallMethodWithTwoParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(int i, bool b){
    WriteInt(i);
    if(b){
        WriteInt(0);
    }
}
void Main(){
    test(1, true);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestCompareEqual()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    if(1==1){
        WriteInt(1);
        i = 1;
    }
    if('c'=='c'){
        WriteInt(1);
    }
    if(""AA""==""AA""){
        WriteInt(1);
    }
    
    if(i == 1){
        WriteInt(1);
    }
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestCompareUnequal()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    if(1!=2){
        WriteInt(1);
        i = 1;
    }
    if('c'!='d'){
        WriteInt(1);
    }
    if(""AA""!=""A2A""){
        WriteInt(1);
    }
    
    if(i != 2){
        WriteInt(1);
    }
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestCompare_LT_GT()
        {
            var output = RunForOutput(@"
class P{
void Main(){

    if(1<2){
        WriteInt(1);
    }

    if(1<=2){
        WriteInt(1);
    }
    if(2>=2){
        WriteInt(1);
    }
    if(3>2){
        WriteInt(1);
    }    
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestFieldAccess()
        {
            var output = RunForOutput(@"
class P{

int i;

void pp(){

    i=10;

    WriteInt(i);
}

void Main(){
    pp();
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestReturn()
        {
            var output = RunForOutput(@"
class P{
int pp(){
    return 5*5;
}

void Main(){
    int i;
    i = pp();
    WriteInt(i);
}}
");

            Assert.AreEqual("25", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestNegativeMax()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    i = -2147483648;
    WriteInt(i);
}}
");

            Assert.AreEqual("-2147483648", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestArray()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int[] i;
    i = new int[10];
    i[0] = 1;
    i[2] = 2;
    WriteInt(i[0]);
    WriteInt(i[2]);
}}
");

            Assert.AreEqual("12", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestAnd()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    if(true && true){
    WriteInt(1);
    }
    if(false && true){
    WriteInt(1);
    }
    if(true && false){
    WriteInt(1);
    }
    if(false && false){
    WriteInt(1);
    }
if(true && true && true && true && false && true){
    WriteInt(1);
    }
if(false && false && false && false && true){
    WriteInt(1);
    }
if(true && true && true && true && false){
    WriteInt(1);
    }
WriteInt(2);
}}
");

            Assert.AreEqual("12", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestOr()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    if(true || true){
    WriteInt(1);
    }
    if(false || true){
    WriteInt(1);
    }
    if(true || false){
    WriteInt(1);
    }
    if(false || false){
    WriteInt(1);
    }
WriteInt(2);
}}
");

            Assert.AreEqual("1112", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestArrayMulti()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[][][] s;
    s = new string[10][][];
    s[0] = new string[10][];
    s[0][5] = new string[5];
    s[0][5][1] = ""ASDF"";
    WriteString(s[0][5][1]);
}}
");

            Assert.AreEqual("ASDF", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestArrayLen()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[] s;
    s = new string[10];
    s[0] = ""ASDF"";
    WriteInt(s.length);
    WriteString(s[0]);
}}
");

            Assert.AreEqual("10ASDF", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestArrayNull()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[] s;
    s = null;
    if(s == null){
WriteInt(111);
}
else
{
WriteInt(22);
}
}}
");

            Assert.AreEqual("111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestArrayNullInArray()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[][] s;
    s = new string[10][];
    s[0] = null;
    if(s[0] == null){
WriteInt(111);
}
else
{
WriteInt(22);
}
}}
");

            Assert.AreEqual("111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void WriteLine()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    WriteString(""TEST"");
}}
");

            Assert.AreEqual("TEST", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignInt()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 1;
    WriteInt(i);
}}
");

            Assert.AreEqual("1", output);
        }

        [TestMethod, Ignore]
        [ExpectedException(typeof(VMException))]
        public void Halt()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    WriteString(""AAAA"");
    Halt(""TEST"");
    WriteString(""AAAA"");
}}
");

            Assert.AreNotEqual("AAAAAAAA", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void AssignIntAdd()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 + 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("4", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignIntMult()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 * 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("4", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void AssignIntDiv()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 / 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignIntMinus()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 - 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("0", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void AssignIntMod()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    i = 2 % 2;
    WriteInt(i);
}}
");

            Assert.AreEqual("0", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignBool()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    bool i;
    i = true;
    WriteInt(100);
}}
");

            Assert.AreEqual("100", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void AssignIf()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    if (true) {
        i = 100;
    }
    else
    {
        i = 10;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("100", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignIfElseBranch()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int i;
    if (false) {
        i = 100;
    }
    else
    {
        i = 10;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignWhile()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    bool b; int i;
    b = true; i = 1;
    while (b) {
        i = 10;
        b = false;
    }
    WriteInt(i);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void AssignAssign()
        {
            var output = RunForOutput(@"
class P{ void Main(){
    int a; int b; int c;
    
    a = 1;
    b = a;
    c = b;
    

    WriteInt(a+b+c);
}}
");

            Assert.AreEqual("3", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void CallMethodWithoutParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(){
    WriteInt(1);
}
void Main(){
    test();
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void CallMethodWithParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(int i){
    WriteInt(i);
}
void Main(){
    test(1);
}}
");

            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void CallMethodWithTwoParams()
        {
            var output = RunForOutput(@"
class P{ 
void test(int i, bool b){
    WriteInt(i);
    if(b){
        WriteInt(0);
    }
}
void Main(){
    test(1, true);
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void CompareEqual()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    if(1==1){
        WriteInt(1);
        i = 1;
    }
    if('c'=='c'){
        WriteInt(1);
    }
    if(""AA""==""AA""){
        WriteInt(1);
    }
    
    if(i == 1){
        WriteInt(1);
    }
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void CompareUnequal()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    if(1!=2){
        WriteInt(1);
        i = 1;
    }
    if('c'!='d'){
        WriteInt(1);
    }
    if(""AA""!=""A2A""){
        WriteInt(1);
    }
    
    if(i != 2){
        WriteInt(1);
    }
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void Compare_LT_GT()
        {
            var output = RunForOutput(@"
class P{
void Main(){

    if(1<2){
        WriteInt(1);
    }

    if(1<=2){
        WriteInt(1);
    }
    if(2>=2){
        WriteInt(1);
    }
    if(3>2){
        WriteInt(1);
    }    
}}
");

            Assert.AreEqual("1111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void FieldAccess()
        {
            var output = RunForOutput(@"
class P{

int i;

void pp(){

    i=10;

    WriteInt(i);
}

void Main(){
    pp();
}}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void Return()
        {
            var output = RunForOutput(@"
class P{
int pp(){
    return 5*5;
}

void Main(){
    int i;
    i = pp();
    WriteInt(i);
}}
");

            Assert.AreEqual("25", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void NegativeMax()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    i = -2147483648;
    WriteInt(i);
}}
");

            Assert.AreEqual("-2147483648", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void NegativeMaxIntInWrongPlace()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int i;
    i = 2000-2147483648;
    WriteInt(i);
}}
");

            Assert.IsTrue(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void Array()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int[] i;
    i = new int[10];
    i[0] = 1;
    i[2] = 2;
    WriteInt(i[0]);
    WriteInt(i[2]);
}}
");

            Assert.AreEqual("12", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void And()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    if(true && true){
    WriteInt(1);
    }
    if(false && true){
    WriteInt(1);
    }
    if(true && false){
    WriteInt(1);
    }
    if(false && false){
    WriteInt(1);
    }
if(true && true && true && true && false && true){
    WriteInt(1);
    }
if(false && false && false && false && true){
    WriteInt(1);
    }
if(true && true && true && true && false){
    WriteInt(1);
    }
WriteInt(2);
}}
");

            Assert.AreEqual("12", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void Or()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    if(true || true){
    WriteInt(1);
    }
    if(false || true){
    WriteInt(1);
    }
    if(true || false){
    WriteInt(1);
    }
    if(false || false){
    WriteInt(1);
    }
WriteInt(2);
}}
");

            Assert.AreEqual("1112", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ArrayMulti()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[][][] s;
    s = new string[10][][];
    s[0] = new string[10][];
    s[0][5] = new string[5];
    s[0][5][1] = ""ASDF"";
    WriteString(s[0][5][1]);
    WriteInt(s[0][5].length);
}}
");

            Assert.AreEqual("ASDF5", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ArrayLen()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[] s;
    s = new string[10];
    s[0] = ""ASDF"";
    WriteInt(s.length);
    WriteString(s[0]);
}}
");

            Assert.AreEqual("10ASDF", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ArrayLenInt()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    int[] s;
    s = new int[10];
    s[0] = 2;
    WriteInt(s.length);
    WriteInt(s[0]);
}}
");

            Assert.AreEqual("102", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ArrayNull()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[] s;
    s = null;
    if(s == null){
WriteInt(111);
}
else
{
WriteInt(22);
}
}}
");

            Assert.AreEqual("111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ArrayNullInArray()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    string[][] s;
    s = new string[10][];
    s[0] = null;
    if(s[0] == null){
WriteInt(111);
}
else
{
WriteInt(22);
}
}}
");

            Assert.AreEqual("111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ObjectCreation()
        {
            var output = RunForOutput(@"
class P{
void Main(){
    A a;
    a = new A();
    a.i = 10;
    WriteInt(a.i);
}}

class A{
int i;
}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ObjectFull()
        {
            var output = RunForOutput(@"
class P{
A f;
void Main(){
    A a;
    a = new A();
    a.i = 10;
    WriteInt(a.i);

    P p;
    p = new P();
    p.f = new A();
    p.f.iaa = new int[10][];
    p.f.iaa[0] = new int[10];
    p.f.iaa[0][0] = 33;

    WriteInt(p.f.iaa.length);
    int aaaa;
    aaaa = p.f.call(p);
    WriteInt(aaaa);
    p.f.i = 99;
    WriteInt(p.f.call(p));
}}

class A{
int i;
int[][] iaa;

int call(P param){
    WriteInt(param.f.i);
    return iaa[0][0];
}
}
");


            Assert.AreEqual("10100339933", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ObjectThisReference()
        {
            var output = RunForOutput(@"
class P{void Main(){

    A a;
    a = new A();
    WriteInt(a.call());

}}

class A{
int i;
int b;
int call(){
    this.i = 10;
    i = 10;
    b = this.i;
    return b;
}
}
");

            Assert.AreEqual("10", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ObjectThisReferenceMethod()
        {
            var output = RunForOutput(@"
class P{void Main(){

    A a;
    a = new A();
    a.call();

}}

class A{
void callNew(){
WriteInt(10);
}
void call(){
    this.callNew();
    callNew();
}
}
");

            Assert.AreEqual("1010", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ObjectNull()
        {
            var output = RunForOutput(@"
class P{void Main(){

    A a;
    a = null;

    if (a == null){
        WriteInt(1010);
    }


}}

class A{}
");

            Assert.AreEqual("1010", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ObjectIsInst()
        {
            var output = RunForOutput(@"
class P{void Main(){

    A a;
    a = new A();

    if (a is A){
        WriteInt(1010);
    }


}}

class A{}
");

            Assert.AreEqual("1010", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ObjectClassCast()
        {
            var output = RunForOutput(@"
class P{void Main(){
    A b;
    b = new B();

    b.call();

    B a;
    a = (B) b;
    a.call();
    if (a is B){
        WriteInt(1010);
    }
}}

class A{
    void call(){WriteString(""A"");}
}
class B:A{
    void call(){WriteString(""B"");}
}
");

            Assert.AreEqual("BB1010", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void Arr()
        {
            var output = RunForOutput(@"
class P{void Main(){
    int[] a;
    a = new int[10];
    a[8] = 1234;
}}
");

            Assert.AreEqual("", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ArrParam()
        {
            var output = RunForOutput(@"
class Test { 
void Main() {
    int[] a; 
    a = new int[10]; 
    int index; 
    index = 0; 
    while (index < a.length) { 
        a[index] = index;
        index = index + 1; 
    }
    Print(a);
} 
void Print(int[] a) { 
    int index; 
    index = 0; 
    while (index < a.length) { 
        WriteInt(a[index]);
        index = index + 1; 
    } 
} 
}");

            Assert.AreEqual("0123456789", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void TestArrMulti()
        {
            var output = RunForOutput(@"
class Test{ void Main() { 
    int[][] a; 
    a = new int[10][]; 
    a[0] = new int[2];  
    a[0][1] = 9; 
    WriteInt(a[0][1]);   
}}");

            Assert.AreEqual("9", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void IntegerTooSmall()
        {
            var output = RunForOutput(@"
class Test { 
void Main() {
    int a;
    a = -21474836489;
   }
}");

            Assert.IsTrue(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void IntegerTooBig()
        {
            var output = RunForOutput(@"
class Test {
void Main() {
    int a;
    a = 2147483648;
    WriteInt(a);
}
}"
            );
            Assert.AreEqual("0", output);
            Assert.IsTrue(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void UnaryNot()
        {
            var output = RunForOutput(@"
class Test {
void Main() {
    bool a;
    a = !false;
    if(a) {
        WriteInt(1);
    } else {
        WriteInt(0);
    }
}
}");
            Assert.AreEqual("1", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void Parameter()
        {
            var output = RunForOutput(@"
class Test {
    int func(int a, bool b) {
    int result;
    if(b) {
        result = a;
    } else {
        result = a;
        result = -result;
    }
    return result;
}
    void Main() {
        int num;
        int a; bool b;
        a = 1; b = false;
        num = func(a, b);
        WriteInt(num);
        b = true;
        num = func(a, b);
        WriteInt(num);
    }
}");
            Assert.AreEqual("-11", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ComplexComputation()
        {
            var code = @"
class Mathematical {
    int pow(int numBase, int exponent) {
        int i;
        i = 1;
        int result;
        result = numBase;
        while(i < exponent) {
            result = result * numBase;
            i = i + 1;
        }
        return result;
    }

    int square(int number) {
        return this.pow(number, 2);
   } 
}
class Program {
    void Main() {
        Mathematical m;
        m = new Mathematical();
        int result;
        result = m.pow(5, 6);
        WriteInt(result);
        result = m.square(10);
        WriteInt(result);
    }
}";
            var output = RunForOutput(code);
            Assert.AreEqual("15625100", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ComplexObject()
        {
            var code = @"
class Vehicle {
    void Drive() {
        WriteString(""generic drive"");
    }
}
class Car: Vehicle {
    void Drive() {
        WriteString(""brum brum"");
    }
}
class Truck: Vehicle {
    void Drive() {
        WriteString(""brrrrrr"");
    }
}
class SportsCar: Car {
    void Drive() {
        WriteString(""wrooooom"");
    }
}
class HeavyLoadTruck: Truck {
    void Drive() {
        WriteString(""do you have permission?"");
    }
}

class Program {
    void Main() {
        Vehicle a;
        a = new Vehicle();
        a.Drive();
        a = new Car();
        a.Drive();
        Car b;
        b = new Car();
        b.Drive();
        b = new SportsCar();
        b.Drive();
        Truck t;
        t = new HeavyLoadTruck();
        t.Drive();
    }
}
";
            var output = RunForOutput(code);
            Assert.AreEqual("generic drivebrum brumbrum brumwrooooomdo you have permission?", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void TestOldDataForParserError()
        {
            var output = RunForOutput(@"
class Test{ void Main() { 
    int a; 
    int b;
    int c;
    c = 6;
    b = (6);
    a = (b) + 3;
    WriteInt(a);
    a = (c + c);
    WriteInt(a);
}}");

            Assert.AreEqual("912", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ObjectParameter()
        {
            var code = @"
class Vehicle {
    void Drive() {
        WriteString(""generic drive"");
    }
}
class Car: Vehicle {
    void Drive() {
        WriteString(""brum brum"");
    }
}
class VehicleTransporter {
    void AddVehicle(Vehicle v) {
        v.Drive();
    }
}
class Program {
    void Main() {
        Car c;
        c = new Car();
        VehicleTransporter tr;
        tr = new VehicleTransporter();
        tr.AddVehicle(c);
    }
}";
            var output = RunForOutput(code);
            Assert.AreEqual("brum brum", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ArrayParameter()
        {
            var code = @"
class ArrayHelper {
    bool Empty(int[] array) {
        return array.length == 1;
    }
}
class Program {
    void Main() {
        ArrayHelper a;
        a = new ArrayHelper();
        int[] arr;
        arr = new int[1];
        bool b;
        b = a.Empty(arr);
        if(b) {
            WriteString(""success"");
        }
    }
}";
            var output = RunForOutput(code);
            Assert.AreEqual("success", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void ObjectArray()
        {
            var code = @"
class ObjectHolder {}
class ObjectStore {
    ObjectHolder[] store;
    void Initialize(int size){
        store = new ObjectHolder[size];
    }
    void AddElement(ObjectHolder holder) {
        store[0] = holder;
    }
}
class Program {
    void Main() {
        ObjectStore s;
        s = new ObjectStore();
        ObjectHolder h;
        h = new ObjectHolder();
        s.Initialize(2);
        s.AddElement(h);
    }
}";
            RunForOutput(code);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void ComplexObjectRelations()
        {
            var code = @"
class Vehicle {
    Engine engine;
    void Run() {
        WriteString(""Starting engine\n"");
        engine = new Engine();
        engine.power();
    }
}
class Engine {
    Zylinder[] z;
    void power(){
        z = new Zylinder[3];
        z[0] = new Zylinder();
        z[1] = new Zylinder();
        z[2] = new Dyson();
        int i;int res;

        int[] powers;
        powers = new int[5];

        i= 0;
        while(i<powers.length){
            powers[i] = i+2;
            i=i+1;
        }

        i= 0;
        while(i<z.length){
            res = z[i].powerUp(powers);
            WriteInt(res);WriteString(""\n"");
            i=i+1;
        }
    }
}
class Zylinder {
    int power;
    
    int powerUp(int[] powers){
        int i;int res;
        i= 0;
        while(i<powers.length){
            res=res+powers[i];
            i=i+1;
        }
        power = res;
        return res;
    }
}
class Dyson: Zylinder {
    int powerUp(int[] powers){
        int i;int res;
res = 1;
        i= 0;
        while(i<powers.length){
            res=res*powers[i];
            i=i+1;
        }
        power = res;
        return res;
    }
}

class Program {
    void Main() {
        Vehicle a;
        a = new Vehicle();
        a.Run();
    }
}
";
            var output = RunForOutput(code);
            Assert.AreEqual("Starting engine\n20\n20\n720\n", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }










        [TestMethod]
        public void OverridingMethodsMatch()
        {
            var code = @"
class A{
    int test(){
        return 1;
    }
}

class B:A{
    bool test(){
        return true;
    }
}

class P {
    void Main() {
        A a;
        a = new B();
        WriteInt(a.test());
    }
}
";

            try
            {
                var output = RunForOutput(code);
            }
            catch (Exception) { }
            //Assert.AreEqual("", output);
            Assert.IsTrue(Diagnosis.HasErrors);
        }



        [TestMethod]
        public void OverridingMethodsWithDiffrentLocals()
        {
            var code = @"
class A{
    int test(){
        bool b;
        b = true;
        WriteInt(123);
        return 1;
    }
}

class B:A{
    int test(){
        int i;
        i = 12;
        WriteInt(33);
        return 1;
    }
}

class P {
    void Main() {
        A a;
        a = new B();
        WriteInt(a.test());
    }
}
";
            
            var output = RunForOutput(code);
            Assert.AreEqual("331", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        public void OverridingMethodsWithDiffrentLocalsMultiple()
        {
            var code = @"
class A{
    int test(){
        WriteInt(123);
        return 1;
    }
}

class B:A{
    int test(){
        int i; bool b;
        i = 12;
        b=true;
        WriteInt(i);
        WriteInt(33);
        return 1;
    }
}

class P {
    void Main() {
        A a;
        a = new B();
        WriteInt(a.test());
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("12331", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void OverridingMethodsWithDiffrentLocalsMultipleReverse()
        {
            var code = @"
class A{
    int test(){
        int i; bool b;
        i = 12;
        b = true;
        WriteInt(123);
        return 1;
    }
}

class B:A{
    int test(){
        WriteInt(33);
        return 1;
    }
}

class P {
    void Main() {
        A a;
        a = new B();
        WriteInt(a.test());
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("331", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        [ExpectedException(typeof(VMException))]
        public void CastIndices()
        {
            var code = @"
class A{}

class B:A{}

class C:B{}

class P {
    void Main() {
        A a;
        a = new B();
        B b;
        b = (B) a;

        b = new B();
        a = b;

        C c;
        c = (C) a;
        
        
        
        WriteInt(331);
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("331", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }



        [TestMethod]
        public void Defaults()
        {
            var code = @"
class A{
        P p;
        int i;
        char c; string s;
        int[] arr;
}


class P {
    void Main() {
        P p;
        int i;
        char c; string s;
        int[] arr;
        
        if (p == null) {WriteInt(1);}
        if (arr == null) {WriteInt(1);}
        if (i == 0) {WriteInt(1);}
        if (c == '\0') {WriteInt(1);}
        if (s == """") {WriteInt(1);}


        A a;
        a = new A();

        if (a.p == null) {WriteInt(1);}
        if (a.arr == null) {WriteInt(1);}
        if (a.i == 0) {WriteInt(1);}
        if (a.c == '\0') {WriteInt(1);}
        if (a.s == """") {WriteInt(1);}
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("1111111111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void DefaultsArray()
        {
            var code = @"

class P {
    void Main() {
        P[] p;
        int[] i;
        char[] c; string[] s;
        int[][] arr;
        
        p = new P[10];
        i = new int[10];
        c = new char[10];
        s = new string[10];
        arr = new int[10][];
        

        if (p[0] == null) {WriteInt(1);}
        if (arr[0] == null) {WriteInt(1);}
        if (i[0] == 0) {WriteInt(1);}
        if (c[0] == '\0') {WriteInt(1);}
        if (s[0] == """") {WriteInt(1);}

    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("11111", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }



        [TestMethod]
        public void FieldShadows()
        {
            var code = @"
class A{
    int a;
}
class B:A{
    bool a;
}

class P {
    void Main() {
        B b;
        A a;

        b = new B();
        b.a = true;
        
        a = new B();
        a.a = 1;

        

        if(b.a){WriteInt(22);}
        
        b = (B) a;

        if(b.a){WriteInt(22);}

        WriteInt(a.a);
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("221", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void ReturnOverride()
        {
            var code = @"
class C{}
class D:C{}

class A{
    C get(){return new D();}
}
class B:A{
    C get(){return new C();}
}

class P {
    void Main() {
        B b;
        A a;

        b = new B();
        a = new A();

        C c;
        D d;

        c = a.get();
        d = (D)c;
        c = b.get();
        WriteInt(221);
    }
}
";

            var output = RunForOutput(code);
            Assert.AreEqual("221", output);
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod]
        [ExpectedException(typeof(VMException))]
        public void ArrayIndexOutOfBounds()
        {
            var code = @"
class Program {
    void Main() {
        int[] i;
        i = new int[10];
        WriteInt(i[11]);
    }
}";
            RunForOutput(code);
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod]
        public void Null()
        {
            var code = @"
class A{
    int i;
}
class B{
    A a;
}
class Program {
    void Main() {
        B b1;B b2;
        b1 = new B();
        b2 = new B();
        b2.a = new A();
        if(b2.a.i == 0){
            WriteInt(1);
        }
        b2.a = b1.a;

        if(b2.a == null){
            WriteInt(1);
        }
        
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }


        [TestMethod, Ignore]
        public void NullAccess()
        {
            var code = @"
class A{
    int i;
}
class B{
    A a;
}
class Program {
    void Main() {
        B b1;B b2;
        b1 = new B();
        b2 = new B();
        b2.a = new A();
        if(b2.a.i == 0){
            WriteInt(1);
        }
        b2.a = b1.a;

        if(b2.a == null){
            WriteInt(1);
        }

        if(b2.a.i == 0){
            WriteInt(1);
        }
        
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void NullAccessWrite()
        {
            var code = @"
class A{
    int i;
}
class B{
    A a;
}
class Program {
    void Main() {
        B b1;B b2;
        b1 = new B();
        b2 = new B();
        b2.a = new A();
        if(b2.a.i == 0){
            WriteInt(1);
        }
        b2.a = b1.a;

        if(b2.a == null){
            WriteInt(1);
        }

        b2.a.i = 1;
        
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void NullAccessWriteElement()
        {
            var code = @"
class A{
    int i;
}
class Program {
    void Main() {
        A[] a;
        a[2] = new A();
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void NullAccessElement()
        {
            var code = @"
class Program {
    void Main() {
        int[] a;
        WriteInt(a[2]);   
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }

        [TestMethod, Ignore]
        public void NullAccessArrayLen()
        {
            var code = @"
class Program {
    void Main() {
        int[] a;
        WriteInt(a.length);   
    }
}";
            var outp = RunForOutput(code);
            Assert.AreEqual(outp, "11");
            Assert.IsFalse(Diagnosis.HasErrors);
        }
    }
}
