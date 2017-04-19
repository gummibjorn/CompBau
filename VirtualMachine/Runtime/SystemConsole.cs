using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.VirtualMachine.Runtime
{
    public interface IConsole
    {
        void Write(object o);
        int Read();
    }

    public class SystemConsole : IConsole
    {

        void IConsole.Write(object o)
        {
            Console.Write(o);
            throw new NotImplementedException();
        }

        int IConsole.Read()
        {
            return Console.Read();
            throw new NotImplementedException();
        }
    }
}
