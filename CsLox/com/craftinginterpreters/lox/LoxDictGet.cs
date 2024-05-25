using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxDictGet : LoxCallable
    {
        readonly LoxDict dict;

        public LoxDictGet(LoxDict dict)
        {
            this.dict = dict;
        }

        public int arity()
        {
            return 1;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            Object key = (Object)arguments[0];
            return dict.elements[key];
        }
    }
}
