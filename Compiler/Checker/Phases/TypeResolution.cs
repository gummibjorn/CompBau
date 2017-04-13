using System;
using RappiSharp.Compiler.Checker.General;
using RappiSharp.Compiler.Checker.Symbols;
using RappiSharp.Compiler.Parser.Tree;
using System.Collections.Generic;

namespace RappiSharp.Compiler.Checker.Phases
{
    internal class TypeResolution
    {
        private readonly SymbolTable _symbolTable;

        public static void Run(SymbolTable symbolTable)
        {
            new TypeResolution(symbolTable).ResolveTypesInSymbols();
        }

        private TypeResolution(SymbolTable symbolTable)
        {
            _symbolTable = symbolTable;
        }

        private void ResolveTypesInSymbols()
        {
            foreach (var classSymbol in _symbolTable.Compilation.Classes)
            {
                ResolveTypesInSymbol(classSymbol);
            }
        }

        private void ResolveTypesInSymbol(ClassSymbol classSymbol)
        {
            var classNode = _symbolTable.GetDeclarationNode<ClassNode>(classSymbol);
            if (classNode.BaseClass != null)
            {
                checkCyclicInheritance(classNode);
                classSymbol.BaseClass = ResolveType(classNode.BaseClass);
            }
            foreach (var field in classSymbol.Fields)
            {
                ResolveTypeInSymbol(field);
            }
            foreach (var method in classSymbol.Methods)
            {
                ResolveTypesInSymbol(method);
            }
        }

        private void checkCyclicInheritance(ClassNode classNode)
        {
            var baseClasses = new HashSet<String>();

            while(classNode.BaseClass != null)
            {
                if (baseClasses.Add(classNode.Identifier)) { }
                else
                {
                    Diagnosis.ReportError(classNode.Location, "Cyclic inheritance is not allowed");
                    return;
                }


                var classType = ResolveType(classNode.BaseClass);
                if (classType != null)
                {
                    classNode = _symbolTable.GetDeclarationNode<ClassNode>(classType);
                }else
                {
                    return;
                }
            }
        }

        private void ResolveTypeInSymbol(VariableSymbol variable)
        {
            var variableNode = _symbolTable.GetDeclarationNode<VariableNode>(variable);
            variable.Type = ResolveType(variableNode.Type);
        }

        private void ResolveTypesInSymbol(MethodSymbol method)
        {
            var methodNode = _symbolTable.GetDeclarationNode<MethodNode>(method);
            if ((methodNode.ReturnType as BasicTypeNode)?.Identifier != "void")
            {
                method.ReturnType = ResolveType(methodNode.ReturnType);
            }
            foreach (var parameter in method.Parameters)
            {
                ResolveTypeInSymbol(parameter);
            }
            foreach (var local in method.LocalVariables)
            {
                ResolveTypeInSymbol(local);
            }
        }

        private TypeSymbol ResolveType(TypeNode node)
        {
            var result = _symbolTable.FindType(node);
            if (result == null)
            {
                Error(node.Location, "Undeclared type: " + node.ToString());
            }
            return result;
        }

        private void Error(Location location, string message)
        {
            Diagnosis.ReportError(location, $"{message}");
        }
    }
}
