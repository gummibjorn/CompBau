using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.Compiler.Lexer.Tokens
{
    internal class ErrorToken : Token
    {
        public string Message { get; }

        public ErrorToken(Location location, string message) : base(location)
        {
            Message = message;            
        }

        public override string ToString() {
          return $"INTEGER {Message}";
        }
    }
}
