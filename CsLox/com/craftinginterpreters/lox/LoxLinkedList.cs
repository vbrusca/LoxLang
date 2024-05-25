using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxLinkedList : LoxInstance
    {
        internal readonly List<Object> elements;

        internal LoxLinkedList() : base(null)
        {
            elements = new List<Object>();
        }

        internal override Object get(Token name)
        {
            if (name.lexeme.Equals("get"))
            {
                return new LoxLinkedListGet(this);
            }
            else if (name.lexeme.Equals("set"))
            {
                return new LoxLinkedListSet(this);
            }
            else if (name.lexeme.Equals("add"))
            {
                return new LoxLinkedListAdd(this);
            }
            else if (name.lexeme.Equals("length"))
            {
                return (double)elements.Count;
            }

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        internal override void set(Token name, Object value)
        {
            throw new RuntimeError(name, "Can't add properties to arrays.");
        }

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("(");
            for (int i = 0; i < elements.Count; i++)
            {
                if (i != 0)
                {
                    buffer.Append(", ");
                }
                buffer.Append(elements[i]);
            }
            buffer.Append(")");
            return buffer.ToString();
        }
    }
}
