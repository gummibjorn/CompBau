using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Phases;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.Compiler.Checker.Visitors;
using System.Linq;
using RappiSharp.Compiler.Checker.Symbols;
using System;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Checker
{
    internal class RappiChecker
    {
        private readonly ProgramNode _syntaxTree;

        public SymbolTable SymbolTable { get; } = new SymbolTable();

        public RappiChecker(ProgramNode syntaxTree)
        {
            try
            {
                _syntaxTree = syntaxTree;
                SymbolConstruction.Run(SymbolTable, _syntaxTree);
                TypeResolution.Run(SymbolTable);
                FixSyntaxTree();
                CheckSingleMain();
                CheckInvalidOverrides();
            } catch (CheckerException)
            {
                Diagnosis.ReportError(new Location(1, 1), "Checker terminated with errors");
            }
        }

        private void FixSyntaxTree()
        {
            foreach (var classSymbol in SymbolTable.Compilation.Classes)
            {
                foreach (var method in classSymbol.Methods)
                {
                    var methodNode = SymbolTable.GetDeclarationNode<MethodNode>(method);
                    var body = methodNode.Body;
                    body.Accept(new DesignatorFixupVisitor(SymbolTable, method));
                    body.Accept(new TypeCheckVisitor(SymbolTable, method));
                }
            }
        }

        private void CheckSingleMain()
        {
            var location = new Location(-1, -1);
            var entries =
              from classSymbol in SymbolTable.Compilation.Classes
              from method in classSymbol.Methods
              where method.Identifier == "Main"
              select method;
            if (entries.Count() == 0)
            {
                Diagnosis.ReportError(location, "Main method missing");
            }
            else if (entries.Count() > 1)
            {
                Diagnosis.ReportError(location, "Only one Main() method is permitted");
            }
            else
            {
                var main = entries.Single();
                SymbolTable.Compilation.MainMethod = main;
                if (main.Parameters.Count > 0)
                {
                    Diagnosis.ReportError(location, "Main method must have no parameters");
                }
                if (main.ReturnType != null)
                {
                    Diagnosis.ReportError(location, "Main method must be void");
                }
            }
        }

        private void CheckInvalidOverrides()
        {
            foreach(var classSymbol in SymbolTable.Compilation.Classes)
            {
                CheckInvalidOverrides(classSymbol);
            }

        }

        private void CheckInvalidOverrides(ClassSymbol classSymbol)
        {
            if(classSymbol.BaseClass != null)
            {
                var subMethods = new Dictionary<String, MethodSymbol>();
                foreach(var method in classSymbol.Methods)
                {
                    subMethods[method.Identifier] = method;
                }

                var baseMethods = new Dictionary<String, MethodSymbol>();
                foreach(var method in ((ClassSymbol)classSymbol.BaseClass).Methods)
                {
                    baseMethods[method.Identifier] = method;
                }

                foreach(var name in baseMethods.Keys)
                {
                    if (subMethods.ContainsKey(name))
                    {
                        var subMethod = subMethods[name];
                        var baseMethod = baseMethods[name];

                        if(subMethod.Parameters.Count != baseMethod.Parameters.Count)
                        {
                            Diagnosis.ReportError(new Location(-1,-1), $"Parameter count of {classSymbol.Identifier}.{name} does not match base class");
                        }
                        if(subMethod.ReturnType != baseMethod.ReturnType)
                        {
                            Diagnosis.ReportError(new Location(-1,-1), $"Return type of {classSymbol.Identifier}.{name} does not match base class");
                        }
                        for(var i = 0; i < subMethod.Parameters.Count; i++)
                        {
                            if(subMethod.Parameters[i].Type != baseMethod.Parameters[i].Type)
                            {
                                Diagnosis.ReportError(new Location(-1,-1), $"Parameter {i} type of {classSymbol.Identifier}.{name} does not match base class");
                            }
                        }
                    }

                }

                CheckInvalidOverrides(classSymbol.BaseClass as ClassSymbol);


            }
        }
    }
}
