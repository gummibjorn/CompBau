using Microsoft.VisualStudio.TestTools.UnitTesting;
using RappiSharp.Compiler.Parser.Tree;
using RappiSharp.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Test.Generator
{
        internal class ILIterator {
            private List<Instruction> _code;
            int _index = 0;

            public ILIterator(List<Instruction> code)
            {
                _code = code;
            }
            
            public ILIterator Next(OpCode opCode)
            {
                Assert.AreEqual(_code[_index].OpCode, opCode);
                _index += 1;
                return this;
            }

            public ILIterator Next(OpCode opCode, Object operand)
            {
                Assert.AreEqual(_code[_index].OpCode, opCode);
                Assert.AreEqual(_code[_index].Operand, operand);
                _index += 1;
                return this;
            }

            public ILIterator Next(Action<OpCode, Object> action)
            {
                action(_code[_index].OpCode, _code[_index].Operand);
                _index += 1;
                return this;
            }

            public void Return()
            {
                Next(OpCode.ret);
                End();
            }

            private void End()
            {
                if(_index < _code.Count)
                {
                    string instructions = "";
                    for(var i = _index; i < _code.Count; i++)
                    {
                        instructions += _code[i].ToString() + "; ";
                    }
                    Assert.Fail("Expected end, but got more instructions: " + instructions);
                }

            }
        }

}
