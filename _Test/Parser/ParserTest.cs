using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using RappiSharp.Compiler.Parser;
using System;
using System.IO;

namespace _Test
{
    [TestClass]
    public class ParserTest : AbstractTest{
        protected override string getPathInProject() { return "Parser"; }

        private RappiParser _parser;

        private void initializeParser()
        {
            _parser = new RappiParser(makeLexer());
        }

        private void initializeParser(string code)
        {
            _parser = new RappiParser(makeLexer(code));
        }

        [TestMethod]
        public void FunctionChaining()
        {
            initializeParser("class Test { void Main() { a().b().c(); } }");
            _parser.ParseProgram();
            AssertDiagnosisContains("");
        }

        [TestMethod]
        public void ClassEmpty()
        {
            initializeParser("class Foo{}");
            _parser.ParseProgram();
        }

        [TestMethod]
        public void ClassNoname()
        {
            initializeParser("class{}");
            _parser.ParseProgram();
            AssertDiagnosisContains("Identifier expected");
        }

        [TestMethod]
        public void MainMethod()
        {
            initializeParser(main(""));
            _parser.ParseProgram();
        }

        [TestMethod]
        public void IfStatement()
        {
            initializeParser(main("if(0){} else {}"));
            _parser.ParseProgram();
        }

        [TestMethod]
        public void WhileStatement()
        {
            initializeParser(main("while(0){}"));
            _parser.ParseProgram();
        }

        [TestMethod]
        public void ReturnStatement()
        {
            initializeParser(main("return 0;"));
            _parser.ParseProgram();
        }

        [TestMethod]
        public void ExprIntLiteral()
        {
            initializeParser(expression("0"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprAddition()
        {
            initializeParser(expression("0+1"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprAdditionParenthesis()
        {
            initializeParser(expression("0+(1*5)"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMulitplication()
        {
            initializeParser(expression("1*5"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMulitplicationParenthesis()
        {
            initializeParser(expression("(1*5)"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMulitplicationCombined()
        {
            initializeParser(expression("(1*5)-5"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprInvalidMult()
        {
            initializeParser(expression("1 ** 2"));
            try
            {
                _parser.ParseProgram();
            }catch(Exception) {
                AssertDiagnosisContains("Invalid operand: TOKEN Times");
            }
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorSingle()
        {
            initializeParser(expression("i"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorArrayContinued()
        {
            initializeParser(expression("test.i[1].peter"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorArrayContinuedMultiple()
        {
            initializeParser(expression("test.i[1].peter[2]"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorArray()
        {
            initializeParser(expression("test.i[1]"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorArrayEmptyExpression()
        {
            initializeParser(expression("i[]"));
            _parser.ParseProgram();
            AssertDiagnosisContains("Invalid operand");
        }

        [TestMethod, Timeout(1000)]
        public void ExprDesignatorMulti()
        {
            initializeParser(expression("test.i"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMethodCall()
        {
            initializeParser(expression("i()"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMethodCallArrayDesignator()
        {
            initializeParser(expression("i[1].test()"));
            _parser.ParseProgram();
        }

        [TestMethod, Timeout(1000)]
        public void ExprMethodCallMultiDesignator()
        {
            initializeParser(expression("test.i()"));
            _parser.ParseProgram();
        }
    }

}
