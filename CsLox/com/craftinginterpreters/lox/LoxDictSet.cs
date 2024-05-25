using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxDictSet : LoxCallable
    {
        readonly LoxDict dict;

        public LoxDictSet(LoxDict dict)
        {
            this.dict= dict;
        }

        public int arity()
        {
            return 2;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            Object key = arguments[0];
            Object value = arguments[1];
            Object ret = null;

            if (dict.elements.ContainsKey(key))
            {
                ret = dict.elements[key];
            }

            dict.elements.Add(key, value);
            return ret;
        }
    }
}
