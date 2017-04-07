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

namespace _Test
{
    [TestClass]
    public class GeneratorRVMTest : AbstractTest
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
                Assert.IsTrue(str.Contains(expectedOutput), $"Expected to find '{expectedOutput}' in: '{str}'");
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
        public void CallMemberReturn()
        {
            initializeGenerator();
            Run();
        }

        [TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator(expression("int[]", "new int[10]"));
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
            Run("AFalse");
        }
    }
}