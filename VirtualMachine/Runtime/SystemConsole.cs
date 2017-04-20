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
        string ReadLine();
    }

    public class SystemConsole : IConsole
    {

        void IConsole.Write(object o)
        {
            Console.Write(o);
        }

        int IConsole.Read()
        {
            return Console.Read();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
