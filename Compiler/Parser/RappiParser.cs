using RappiSharp.Compiler.Lexer;
using RappiSharp.Compiler.Lexer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RappiSharp.Compiler.Parser
{
    class RappiParser
    {
        Token _current;
        RappiLexer _lexer;

        public RappiParser(RappiLexer lexer)
        {
            _lexer = lexer;
        }

        private void ReadNext()
        {
            _current = _lexer.Next();
        }

        bool Is(Tag tag)
        {
            return _current is FixToken &&
                ((FixToken)_current).Tag == tag;
        }

        bool IsInteger()
        {
            return _current is IntegerToken;
        }

        bool IsIdentifier()
        {
            return _current is IdentifierToken;
        }

        bool IsEnd()
        {
            return Is(Tag.End);
        }

        void ReportError(string message = "")
        {
            Diagnosis.ReportError(new Location(0,0), message);

        }

        public void Parse()
        {
            ReadNext();
            while (Is(Tag.Class))
            {
                ParseClass();
            }
            if (!IsEnd())
            {
                ReportError($"Expected end, but {_current.ToString()} found");
            }

        }

        private void ExpectNext(Tag tag)
        {
            ReadNext();
            if (!Is(tag))
            {
                ReportError($"Expected {tag} but got ${_current}");
            }

        }

        public void ParseClass()
        {
            ReadNext();
            if (IsIdentifier())
            {
                ReadNext();
                if (Is(Tag.Colon)) //extends
                {
                    ReadNext();
                    if (IsIdentifier())
                    {
                        ReadNext();
                        //this is the base class name
                    } else
                    {
                        ReadNext();
                        ReportError();
                    }
                }
                if (Is(Tag.OpenBrace))
                {
                    ReadNext();
                    while(IsIdentifier())
                    {
                        ParseType();
                        if (IsIdentifier())
                        {
                            var name = ((IdentifierToken)_current).Name;
                            ReadNext();
                            //ignores the MethodHead of the EBNF and just merges it with MethodBody
                            if (Is(Tag.OpenParenthesis))
                            {
                                ParseMethod();
                            } else if (Is(Tag.Colon))
                            {
                                ReadNext();
                                //this is a variable declaration
                            }

                        }
                    }
                    if (Is(Tag.CloseBrace))
                    {
                        ReadNext();
                        //class OK
                    } else
                    {
                        ReportError();
                        ReadNext();
                    }

                }



            } else
            {
                ReportError("Expected identifier");
            }
        }

        public void ParseType()
        {
            if (IsIdentifier())
            {
                var type = ((IdentifierToken)_current).Name;
                var isArrayType = false;
                ReadNext();
                if (Is(Tag.OpenBracket))
                {
                    ReadNext();
                    if (Is(Tag.CloseBracket))
                    {
                        isArrayType = true;
                    } else
                    {
                        ReportError("Expected CloseBracket");
                    }
                }
            } else
            {
                ReportError("Expected identifier");
            }
        }

        public void ParseMethod()
        {
            //TODO
        }
    }
}
