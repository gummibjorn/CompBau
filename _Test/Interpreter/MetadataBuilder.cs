using RappiSharp.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Test
{
        internal class MetadataBuilder
        {
            private MethodData _mainMethod = new MethodData();

            internal Metadata build()
            {
                var _metaData = new Metadata();

                var mainClass = new ClassData();
                mainClass.Identifier = "Program";
                mainClass.Methods.Add(0);

                _mainMethod.Identifier = "Main";
                this.addMainInst(OpCode.ret);

                _metaData.Types.Add(mainClass);
                _metaData.Methods.Add(_mainMethod);
                _metaData.MainMethod = 0;

                return _metaData;
            }

            internal MetadataBuilder addMainLocal(int type)
            {
                _mainMethod.LocalTypes.Add(type);
                return this;
            }

            internal MetadataBuilder addMainInst(OpCode opCode, object operand = null)
            {
                var instruction = new Instruction();
                instruction.OpCode = opCode;
                instruction.Operand = operand;
                _mainMethod.Code.Add(instruction);
                return this;
            }
        }
}
