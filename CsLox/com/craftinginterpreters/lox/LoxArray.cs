using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxArray : LoxInstance
    {
        internal readonly Object[] elements;

        internal LoxArray(int size) : base(null)
        {
            elements = new Object[size];
        }

        internal override Object get(Token name)
        {
            if (name.lexeme.Equals("get"))
            {
                return new LoxArrayGet(this);
            }
            else if (name.lexeme.Equals("set"))
            {
                return new LoxArraySet(this);
            }
            else if (name.lexeme.Equals("length"))
            {
                return (double)elements.Length;
            }

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        internal override void set(Token name, Object value)
        {
            throw new RuntimeError(name, "Can't add properties to arrays.");
        }

        public override String ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("[");
            for (int i = 0; i < elements.Length; i++)
            {
                if (i != 0)
                {
                    buffer.Append(", ");
                }
                buffer.Append(elements[i]);
            }
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}
