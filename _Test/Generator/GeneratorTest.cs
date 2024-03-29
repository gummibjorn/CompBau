﻿using _Test.Generator;
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

namespace _Test
{
    [TestClass]
    public class GeneratorTest : AbstractTest
    {
        protected override string getPathInProject() { return "Generator"; }

        private RappiGenerator _generator;

        private ILIterator initializeGenerator(RappiLexer lexer)
        {
            _generator = new RappiGenerator(new RappiChecker(new RappiParser(lexer).ParseProgram()).SymbolTable);
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

        [TestCleanup]
        public override void tearDown()
        {
            base.tearDown();
            //if serialization fails, we messed up
            var serializer = new XmlSerializer(typeof(Metadata));
            var writer = new StringWriter();
            serializer.Serialize(writer, _generator.Metadata);
        }

        [TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator(expression("int[]", "new int[10]"))
                .Next(ldc_i, 10)
                .Next(newarr)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignLocal()
        {
            initializeGenerator(main("int a; a=10;"))
                .Next(ldc_i, 10)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignLocalBoolTrue()
        {
            initializeGenerator(main("bool a; a=true;"))
                .Next(ldc_b, true)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignLocalBoolFalse()
        {
            initializeGenerator(main("bool a; a=false;"))
                .Next(ldc_b, false)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void AssignParam()
        {
            initializeGenerator(program("void Main(){} void A(int i){ i = 10; }"));
            method("A")
                .Next(ldc_i, 10)
                .Next(starg, 0);
        }

        [TestMethod]
        public void IfStatement()
        {
            initializeGenerator(main("int a; if(true){a=0;}else{a=1;}"))
                .Next(ldc_b, true)
                .Next(brfalse, 3)
                .Next(ldc_i, 0)
                .Next(stloc, 0)
                .Next(br, 2)
                .Next(ldc_i, 1)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void WhileStatement()
        {
            initializeGenerator(main("int a; while(true){a=0;}"))
                .Next(ldc_b, true)
                .Next(brfalse, 3)
                .Next(ldc_i, 0)
                .Next(stloc, 0)
                .Next(br, -5)
                .Return();
        }

        [TestMethod]
        public void BuiltinWriteInt()
        {
            initializeGenerator(main("WriteInt(0);"))
                .Next(ldc_i,0)
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void BuiltinWriteChar()
        {
            initializeGenerator(main("WriteChar('!');"))
                .Next(ldc_c,'!')
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void BuiltinWriteString()
        {
            initializeGenerator(main("WriteString(\"HELLO\");"))
                .Next(ldc_s, "HELLO")
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void BuiltinHalt()
        {
            initializeGenerator(main("Halt(\"HELLO\");"))
                .Next(ldc_s, "HELLO")
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void BuiltinReadInt()
        {
            initializeGenerator(expression("int", "ReadInt()"))
                .Next(call)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionOr()
        {
            initializeGenerator(main("bool a; a = true || false;"))
                .Next(ldc_b, true)
                .Next(brtrue, 4)
                .Next(ldc_b, false)
                .Next(brtrue, 2)
                .Next(ldc_b, false)
                .Next(br, 1)
                .Next(ldc_b, true)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionAnd()
        {
            initializeGenerator(main("bool a; a = true && true;"))
                .Next(ldc_b, true)
                .Next(brfalse, 4)
                .Next(ldc_b, true)
                .Next(brfalse, 2)
                .Next(ldc_b, true)
                .Next(br, 1)
                .Next(ldc_b, false)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionDivide()
        {
            initializeGenerator(main("int a; a = 1/3;"))
                .Next(ldc_i, 1)
                .Next(ldc_i, 3)
                .Next(OpCode.div)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionTimes()
        {
            initializeGenerator(main("int a; a = 1*3;"))
                .Next(ldc_i, 1)
                .Next(ldc_i, 3)
                .Next(OpCode.mul)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionModulo()
        {
            initializeGenerator(main("int a; a = 10%2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.mod)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionMinus()
        {
            initializeGenerator(main("int a; a = 10-2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.sub)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionPlus()
        {
            initializeGenerator(main("int a; a = 10+2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.add)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionLess()
        {
            initializeGenerator(main("bool a; a = 10<2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.clt)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionLessEqual()
        {
            initializeGenerator(main("bool a; a = 10<=2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.cle)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionGreater()
        {
            initializeGenerator(main("bool a; a = 10>2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.cgt)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionGreaterEqual()
        {
            initializeGenerator(main("bool a; a = 10>=2;"))
                .Next(ldc_i, 10)
                .Next(ldc_i, 2)
                .Next(OpCode.cge)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionEquals()
        {
            initializeGenerator(main("bool a; a = true == true;"))
                .Next(ldc_b, true)
                .Next(ldc_b, true)
                .Next(OpCode.ceq)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BinaryExpressionNotEquals()
        {
            initializeGenerator(main("bool a; a = true != true;"))
                .Next(ldc_b, true)
                .Next(ldc_b, true)
                .Next(OpCode.cne)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void UnaryExpressionMinus()
        {
            initializeGenerator(main("int a; a = -2; a = -a;"))
                .Next(ldc_i, 2)
                .Next(OpCode.neg)
                .Next(stloc, 0)
                .Next(ldloc, 0)
                .Next(OpCode.neg)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void UnaryExpressionNot()
        {
            initializeGenerator(main("bool a; a = !true;"))
                .Next(ldc_b, true)
                .Next(OpCode.neg)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void UnaryExpressionPlus()
        {
            initializeGenerator(main("int a; a = +2;"))
                .Next(ldc_i, 2)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BuiltinReadChar()
        {
            initializeGenerator(expression("char", "ReadChar()"))
                .Next(call)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void BuiltinReadString()
        {
            initializeGenerator(expression("string", "ReadString()"))
                .Next(call)
                .Next(stloc, 0)
                .Return();
        }

        [TestMethod]
        public void CallMemberMethod()
        {
            initializeGenerator(program("void Main(){ A(); } void A(){}"))
                .Next(ldthis)
                .Next(callvirt, 1)
                .Return();
        }

        [TestMethod]
        public void CallMemberReturn()
        {
            initializeGenerator()
                .Next(ldthis)
                .Next(callvirt, 1)
                .Next(stloc, 0)
                .Return();

            method("One")
                .Next(ldc_i, 1)
                .Return();

        }

        [TestMethod]
        public void CallMemberArg()
        {
            initializeGenerator()
                .Next(ldthis)
                .Next(ldc_i, 1)
                .Next(callvirt, 1)
                .Return();

            method("A")
                .Next(ldarg, 0)
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void CallMemberMethodComplex()
        {
            initializeGenerator()
                .Next(ldthis)
                .Next(ldc_i, 1)
                .Next(ldc_c, '!')
                .Next(callvirt, 1)
                .Next(stloc, 0)
                .Next(ldloc, 0)
                .Next(call)
                .Return();

            method("A")
                .Next(ldarg, 0)
                .Next(call)
                .Next(ldarg, 1)
                .Next(call)
                .Next(ldc_s, "yay")
                .Return();
        }

        [TestMethod]
        public void MemberAccessThisImplicitPrimitive()
        {
            initializeGenerator(program("int i; void Main(){ i = 5; WriteInt(i); }"))
                .Next(ldthis)
                .Next(ldc_i, 5)
                .Next(stfld, 0)
                .Next(ldthis)
                .Next(ldfld, 0)
                .Next(call)
                .Return();
        }

        [TestMethod]
        public void MemberAccessThisPrimitive()
        {
            initializeGenerator(program("int i; void Main(){ this.i = 5; WriteInt(this.i); }"))
                .Next(ldthis)
                .Next(ldc_i, 5)
                .Next(stfld, 0)
                .Next(ldthis)
                .Next(ldfld, 0)
                .Next(call)
                .Return();
        }
    }
}