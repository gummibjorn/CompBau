using RappiSharp.VirtualMachine.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.VirtualMachine.Runtime
{
    class ClassObject
    {
        public ClassObject(ClassDescriptor type)
        {
            Type = type;
            Fields = Interpreter.InitializedVariables(type.FieldTypes);
        }
        public ClassDescriptor Type { get; }
        public object[] Fields { get; }
    }

    class ArrayObject
    {
        public ArrayObject(ArrayDescriptor type, int length)
        {
            Type = type;
            Elements = new object[length];
            for(var i = 0; i< Elements.Length; i++){
                Elements[i] = Interpreter.DefaultValue(type.ElementType);
            }
        }



        public ArrayDescriptor Type { get; }
        public object[] Elements { get; }
    }
}
