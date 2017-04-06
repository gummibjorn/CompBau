using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Checker;
using RappiSharp.Compiler.Generator;
using RappiSharp.Compiler.Parser;

namespace _Test
{
    [TestClass]
    public class GeneratorTest : AbstractTest
    {
        protected override string getPathInProject() { return "Generator"; }

        private RappiGenerator _generator;

        private void initializeGenerator()
        {
            _generator = new RappiGenerator(new RappiChecker(new RappiParser(makeLexer()).ParseProgram()).SymbolTable);
        }

        private void initializeGenerator(string code)
        {
            _generator = new RappiGenerator(new RappiChecker(new RappiParser(makeLexer(code)).ParseProgram()).SymbolTable);
        }

        private string expression(string type, string expr)
        {
            return main($"{type} a; a = {expr};");
        }

        /*[TestMethod]
        public void ArrayCreation()
        {
            initializeGenerator();
            _generator.Metadata.Methods[0].Code[0].OpCode = RappiSharp.IL.OpCode.ldc_i;
            _generator.Metadata.Methods[0].Code[0].Operand = 1;
            _generator.Metadata.Methods[0].Code[1]. = RappiSharp.IL.OpCode.newarr;

        }*/

    }
}