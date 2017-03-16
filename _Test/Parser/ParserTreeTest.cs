using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Parser.Tree;
using System.IO;
using System.Collections.Generic;

namespace _Test
{
    [TestClass]
    public class ParserTreeTest
    {
        const int TIMEOUT = 1000;
        private Location L = new Location(-1, -1);
        private RappiParser _parser;

        public TestContext TestContext { get; set; }

        private void initializeParser()
        {
            var path = "../../Parser/" + TestContext.TestName + ".txt";
            var lexer = new RappiLexer(File.OpenText(path));
            Diagnosis.source = File.ReadAllText(path);
            _parser = new RappiParser(lexer);
        }

        private void initializeParser(string code)
        {
            var lexer = new RappiLexer(new StringReader(code));
            Diagnosis.source = code;
            _parser = new RappiParser(lexer);
        }

        private string main(string content)
        {
            return "class Proram{ void main(){" + content + "}}";
        }

        private string expression(string expr)
        {
            return main("return " + expr + ";");
        }

        class ExpressionExtractor : Visitor
        {
            public ExpressionNode _expression;
            public override void Visit(ReturnStatementNode node)
            {
                _expression = node.Expression;
            }
        }

        private ExpressionNode getExpression(ProgramNode node)
        {
            var extractor = new ExpressionExtractor();
            node.Accept(extractor);
            return extractor._expression;
        }

        private void AssertDiagnosisContains(string msg)
        {
            Assert.IsTrue(Diagnosis.Messages.Contains(msg), $"Expected diagnosis to contain '${msg}', but was '${Diagnosis.Messages}'");
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

        [TestInitialize]
        public void setUp()
        {
            Diagnosis.Clear();
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassEmpty()
        {
            initializeParser("class Foo{}");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>(),
                    new List<MethodNode>()
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithMultipleVariables()
        {
            initializeParser("class Foo{ int[][] i; char c; }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() {
                        new VariableNode(L,
                            new ArrayTypeNode(L, new ArrayTypeNode(L, new BasicTypeNode(L, "int"))),
                            "i"
                        ),
                        new VariableNode(L,
                            new BasicTypeNode(L, "char"),
                            "c"
                        )
                    },
                    new List<MethodNode>()
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithNestedArrayVariable()
        {
            initializeParser("class Foo{ int[][] i; }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() {
                        new VariableNode(L,
                            new ArrayTypeNode(L, new ArrayTypeNode(L, new BasicTypeNode(L, "int"))),
                            "i"
                        )
                    },
                    new List<MethodNode>()
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithArrayVariable()
        {
            initializeParser("class Foo{ int[] i; }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() {
                        new VariableNode(L,
                            new ArrayTypeNode(L, new BasicTypeNode(L, "int")),
                            "i"
                        )
                    },
                    new List<MethodNode>()
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithSimpleVariable()
        {
            initializeParser("class Foo{ int i; }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() {
                        new VariableNode(L,
                            new BasicTypeNode(L, "int"),
                            "i"
                        )
                    },
                    new List<MethodNode>()
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }
    }
}
