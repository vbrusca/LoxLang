using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxInstance
    {
        private LoxClass klass;
        private readonly Dictionary<String, Object> fields = new Dictionary<String, Object>();

        internal LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        internal virtual Object get(Token name)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            LoxFunction method = klass.findMethod(name.lexeme);
            if (method != null) return method.bind(this);

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        internal virtual void set(Token name, Object value)
        {
            fields[name.lexeme] = value;
        }

        public override String ToString()
        {
            return klass.name + " instance";
        }
    }
}
