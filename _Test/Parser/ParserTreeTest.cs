﻿﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler;
using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Parser;
using RappiSharp.Compiler.Parser.Tree;
using System.IO;
using System.Collections.Generic;

namespace _Test
{
    [TestClass]
    public class ParserTreeTest : AbstractTest
    {
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

        [TestMethod, Timeout(TIMEOUT)]
        public void ClassEmpty()
        {
            initializeParser("class Foo{}");
            var result = _parser.ParseProgram();
            var expected = new ProgramNode(L,
                new List<ClassNode>(){
                    new ClassNode(L, "Foo",
                    null,
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

        [TestMethod]
        public void StatementCallWithArgument()
        {
            initializeParser(main("hans(1);"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new CallStatementNode(L,
                new MethodCallNode(L,
                    new BasicDesignatorNode(L, "hans"),
                    new List<ExpressionNode>() {
                        new IntegerLiteralNode(L, 1),
                    }
                )
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementCallWithArgumentsInvalid()
        {
            initializeParser(main("hans(,,,);"));
            var result = getFirstStatement(_parser.ParseProgram());
            AssertDiagnosisContains("");
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void StatementCallWithArguments()
        {
            initializeParser(main("hans(1, a, (1+2), arrr[1]);"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = new CallStatementNode(L,
                new MethodCallNode(L,
                    new BasicDesignatorNode(L, "hans"),
                    new List<ExpressionNode>() {
                        new IntegerLiteralNode(L, 1),
                        new BasicDesignatorNode(L, "a"),
                        new BinaryExpressionNode(L,
                            new IntegerLiteralNode(L, 1),
                            Operator.Plus,
                            new IntegerLiteralNode(L, 2)
                        ),
                        new ElementAccessNode(L,
                            new BasicDesignatorNode(L, "arrr"),
                            new IntegerLiteralNode(L, 1)
                        )
                    }
                )
            );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod]
        public void StatementAssignmentBasicAndInit()
        {
            initializeParser(main("int i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            AssertDiagnosisContains("Invalid statement");
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
        public void StatementWhileInvalidBody()
        {
            initializeParser(main("while(0){{return;}}"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements[0];
            AssertDiagnosisContains("");
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
        public void StatementEmpty()
        {
            initializeParser(main(";"));
            var result = getStatementBlock(_parser.ParseProgram()).Statements;
            Assert.AreEqual(0, result.Count);
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
        public void TypeCast()
        {
            initializeParser(expression("(int) asdf"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new TypeCastNode(L,new BasicTypeNode(L, "int"), new BasicDesignatorNode(L, "asdf"));
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void IdentifierInParanthesis()
        {
            initializeParser(expression("(asdf)"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BasicDesignatorNode(L, "asdf");
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod]
        public void IdentifierInParanthesisEvil()
        {
            initializeParser(expression("(asdf - a)"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BinaryExpressionNode(L, new BasicDesignatorNode(L, "asdf"),Operator.Minus, new BasicDesignatorNode(L,"a"));
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ExprOperatorPrecedence()
        {
            initializeParser(expression("0 || 1 && 2"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BinaryExpressionNode(L, 
                new IntegerLiteralNode(L, 0),
                Operator.Or,
                new BinaryExpressionNode(L,
                    new IntegerLiteralNode(L, 1),
                    Operator.And,
                    new IntegerLiteralNode(L, 2)
                ) 
                );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ExprOrOperator()
        {
            initializeParser(expression("0 || 0"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BinaryExpressionNode(L, 
                new IntegerLiteralNode(L, 0),
                Operator.Or,
                new IntegerLiteralNode(L, 0)
                );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ExprAndOperator()
        {
            initializeParser(expression("0 && 0"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BinaryExpressionNode(L, 
                new IntegerLiteralNode(L, 0),
                Operator.And,
                new IntegerLiteralNode(L, 0)
                );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ExprNotEquals()
        {
            initializeParser(expression("0 != 0"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new BinaryExpressionNode(L, 
                new IntegerLiteralNode(L, 0),
                Operator.Unequal,
                new IntegerLiteralNode(L, 0)
                );
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ExprArrayCreation()
        {
            initializeParser(expression("new test[1]"));
            var result = getExpression(_parser.ParseProgram());
            var expected = new ArrayCreationNode(L, 
                new BasicTypeNode(L, "test"),
                new IntegerLiteralNode(L, 1)
                );
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
                    null,
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
                    null,
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
                    null,
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
                    null,
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
                    null,
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
                    null,
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
                    null,
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

        [TestMethod, Timeout(TIMEOUT)]
        public void ToStringStatementCallComplex()
        {
            initializeParser(main("hans.peters[0].i();"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "hans.peters[0].i();";
            Assert.AreEqual(expected, result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ToStringStatementAssignmentMemberArrayMember()
        {
            initializeParser(main("hans.peters[0].i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "hans.peters[0].i = 0;";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ToStringStatementAssignmentArrayMember()
        {
            initializeParser(main("peters[0].i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "peters[0].i = 0;";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ToStringStatementAssignmentMemberArray()
        {
            initializeParser(main("hans.i[0] = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "hans.i[0] = 0;";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod]
        public void ToStringStatementAssignmentArray()
        {
            initializeParser(main("i[0] = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "i[0] = 0;";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void ToStringStatementAssignmentMember()
        {
            initializeParser(main("hans.i = 0;"));
            var result = getFirstStatement(_parser.ParseProgram());
            var expected = "hans.i = 0;";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

        [TestMethod, Timeout(TIMEOUT)]
        public void Is()
        {
            initializeParser(expression("foo is Bar"));
            var result = getExpression(_parser.ParseProgram());
            var expected = "(foo Is Bar)";
            Assert.AreEqual(expected.ToString(), result.ToString());
        }

    }
}
