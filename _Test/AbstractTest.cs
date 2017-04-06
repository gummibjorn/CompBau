﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public abstract class AbstractTest
    {
        internal const int TIMEOUT = 1000;
        internal readonly Location L = new Location(1, 1);

        public TestContext TestContext { get; set; }

        protected abstract string getPathInProject();

        internal RappiLexer makeLexer()
        {
            var path = "../../"+getPathInProject()+"/" + TestContext.TestName + ".txt";
            Diagnosis.source = File.ReadAllText(path);
            return new RappiLexer(File.OpenText(path));
        }

        internal RappiLexer makeLexer(string code)
        {
            Diagnosis.source = code;
            return new RappiLexer(new StringReader(code));
        }

        protected string main(string content)
        {
            return "class Program{ void Main(){" + content + "}}";
        }

        protected string expression(string expr)
        {
            return main("return " + expr + ";");
        }

        protected void AssertDiagnosisContains(string msg)
        {
            Assert.IsTrue(Diagnosis.Messages.ToLower().Contains(msg.ToLower()), $"Expected diagnosis to contain '${msg}', but was '${Diagnosis.Messages}'");
            Assert.IsTrue(Diagnosis.HasErrors);
            Diagnosis.Clear();
        }

        [TestCleanup]
        virtual public void tearDown()
        {
            if (Diagnosis.HasErrors)
            {
                Assert.Fail("There are errors in Diagnosis in tearDown():" + Diagnosis.Messages);
            }
        }

        [TestInitialize]
        public void setUp()
        {
            Diagnosis.Clear();
        }
    }
}
