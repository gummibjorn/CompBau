using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Parser.Tree;
using System.IO;
using System.Collections.Generic;
using System;

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

        class StatementBlockExtractor : Visitor
        {
            public StatementBlockNode _node = null;
            public override void Visit(StatementBlockNode node)
            {
                if(_node == null)
                {
                    _node = node;
                }
            }
        }

        private StatementBlockNode getStatementBlock(ProgramNode program)
        {
            var extractor = new StatementBlockExtractor();
            extractor.Visit(program);
            return extractor._node;
        }

        private StatementNode getFirstStatement(ProgramNode program)
        {
            var extractor = new StatementBlockExtractor();
            extractor.Visit(program);
            if(extractor._node.Statements.Count > 0)
            {
                return extractor._node.Statements[0];
            } else
            {
                Assert.Fail("First statement block contains no statements");
                return null;
            }
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
        public void ExprIntLiteral()
        {
            initializeParser(expression("0"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new IntegerLiteralNode(L, 0);
            Assert.AreEqual(expected.ToString(), result.ToString());
        }


        [TestMethod]
        public void StatementCall()
        {
            initializeParser(main("hans();"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new CallStatementNode(L,
                new MethodCallNode(L,
                    new BasicDesignatorNode(L, "hans"),
                    new List<ExpressionNode>()
                )
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementCallComplex()
        {
            initializeParser(main("hans.peters[0].i();"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new CallStatementNode(L, new MethodCallNode(L,
                new MemberAccessNode(L,
                    new ElementAccessNode(L,
                        new MemberAccessNode(L,
                            new BasicDesignatorNode(L, "i"),
                            "peters"
                        ),
                        new IntegerLiteralNode(L, 0)
                    ),
                    "hans"
                ),
                new List<ExpressionNode>()
            ));
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementAssignmentMemberArrayMember()
        {
            initializeParser(main("hans.peters[0].i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new MemberAccessNode(L,
                    new ElementAccessNode(L,
                        new MemberAccessNode(L,
                            new BasicDesignatorNode(L, "i"),
                            "peters"
                        ),
                        new IntegerLiteralNode(L, 0)
                    ),
                    "hans"
                ),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementAssignmentArrayMember()
        {
            initializeParser(main("peters[0].i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new ElementAccessNode(L,
                    new MemberAccessNode(L,
                        new BasicDesignatorNode(L, "i"),
                        "peters"
                    ),
                    new IntegerLiteralNode(L, 0)
                ),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementAssignmentMemberArray()
        {
            initializeParser(main("hans.i[0] = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new MemberAccessNode(L,
                    new ElementAccessNode(L, 
                        new BasicDesignatorNode(L, "i"),
                        new IntegerLiteralNode(L,0)
                    ),
                    "hans"
                ),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod]
        public void StatementAssignmentArray()
        {
            initializeParser(main("i[0] = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new ElementAccessNode(L, 
                    new BasicDesignatorNode(L, "i"),
                    new IntegerLiteralNode(L,0)
                ),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementAssignmentMember()
        {
            initializeParser(main("hans.i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new MemberAccessNode(L, 
                    new BasicDesignatorNode(L, "i"),
                    "hans"
                ),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod]
        //[TestMethod, Timeout(TIMEOUT)]
        public void StatementAssignmentBasic()
        {
            initializeParser(main("i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new AssignmentNode(L,
                new BasicDesignatorNode(L, "i"),
                new IntegerLiteralNode(L, 0)
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementLocalDeclarationArray()
        {
            initializeParser(main("int[][] i;"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new LocalDeclarationNode(L,
                new VariableNode(L,
                    new ArrayTypeNode(L, new ArrayTypeNode(L, new BasicTypeNode(L, "int"))),
                    "i"
                )
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementLocalDeclaration()
        {
            initializeParser(main("int i;"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new LocalDeclarationNode(L,
                new VariableNode(L, new BasicTypeNode(L, "int"), "i")
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementWhile()
        {
            initializeParser(main("while(0){return;}"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new WhileStatementNode(L,
                new IntegerLiteralNode(L, 0),
                new StatementBlockNode(L, new List<StatementNode>()
                {
                    new ReturnStatementNode(L, null)
                })
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementIfElse()
        {
            initializeParser(main("if(0){}else{return;}"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new IfStatementNode(L,
                new IntegerLiteralNode(L, 0),
                new StatementBlockNode(L, new List<StatementNode>()),
                new StatementBlockNode(L, new List<StatementNode>()
                {
                    new ReturnStatementNode(L, null)
                })
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementIf()
        {
            initializeParser(main("if(0){ return; }"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new IfStatementNode(L,
                new IntegerLiteralNode(L, 0),
                new StatementBlockNode(L, new List<StatementNode>()
                {
                    new ReturnStatementNode(L, null)
                }),
                null
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementReturnExpr()
        {
            initializeParser(main("return 0;"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new ReturnStatementNode(L, new IntegerLiteralNode(L, 0));
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementReturnVoid()
        {
            initializeParser(main("return;"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            var expected = new ReturnStatementNode(L, null);
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithVoidReturn()
        {
            initializeParser("class Foo{ void foo(){ return; } }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() ,
                    new List<MethodNode>()
                    {
                        new MethodNode(L,
                            new BasicTypeNode(L, "void"),
                            "foo",
                            new List<VariableNode>(),
                            new StatementBlockNode(L, new List<StatementNode>() {
                                new ReturnStatementNode(L, null)
                            })
                        )
                    }
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithEmptyMethodParams()
        {
            initializeParser("class Foo{ void foo(int x, char[] y){} }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() ,
                    new List<MethodNode>()
                    {
                        new MethodNode(L, 
                            new BasicTypeNode(L, "void"),
                            "foo",
                            new List<VariableNode>() {
                                new VariableNode(L,
                                    new BasicTypeNode(L, "int"),
                                    "x"
                                ),
                                new VariableNode(L,
                                    new ArrayTypeNode(L, new BasicTypeNode(L, "char")),
                                    "y"
                                )
                            },
                            new StatementBlockNode(L, new List<StatementNode>())
                        )
                    }
                    )
                }
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassWithEmptyMethod()
        {
            initializeParser("class Foo{ void foo(){} }");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    new BasicTypeNode(L, "Object"),
                    new List<VariableNode>() ,
                    new List<MethodNode>()
                    {
                        new MethodNode(L, 
                            new BasicTypeNode(L, "void"),
                            "foo",
                            new List<VariableNode>(),
                            new StatementBlockNode(L, new List<StatementNode>())
                        )
                    }
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
