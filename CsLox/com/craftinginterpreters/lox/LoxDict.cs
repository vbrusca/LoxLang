using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxDict : LoxInstance
    {
        internal readonly Dictionary<Object, Object> elements;

        internal LoxDict() : base(null)
        {
            elements = new Dictionary<Object, Object>();
        }

        internal override Object get(Token name)
        {
            if (name.lexeme.Equals("get"))
            {
                return new LoxDictGet(this);
            }
            else if (name.lexeme.Equals("set"))
            {
                return new LoxDictSet(this);
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
            buffer.Append("<");
            Object[] keys = elements.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                if (i != 0)
                {
                    buffer.Append(", ");
                }
                buffer.Append("(" + keys[i] + " = " + elements[keys[i]] + ")");
            }
            buffer.Append(">");
            return buffer.ToString();
        }
    }
}
