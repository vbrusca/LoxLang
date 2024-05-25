using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace com.craftinginterpreters.lox
{
    internal class LoxClass : LoxCallable
    {
        internal readonly String name;
        private readonly LoxClass superclass;
        private readonly Dictionary<String, LoxFunction> methods;

        internal LoxClass(String name, LoxClass superclass, Dictionary<String, LoxFunction> methods)
        {
            this.superclass = superclass;
            this.name = name;
            this.methods = methods;
        }

        internal LoxFunction findMethod(String name)
        {
            if (methods.ContainsKey(name))
            {
                return methods[name];
            }

            if (superclass != null)
            {
                return superclass.findMethod(name);
            }

            return null;
        }

        public override String ToString()
        {
            return name;
        }

        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = findMethod("init");
            if (initializer != null)
            {
                initializer.bind(instance).call(interpreter, arguments);
            }

            return instance;
        }

        public int arity()
        {
            LoxFunction initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }
    }
}
