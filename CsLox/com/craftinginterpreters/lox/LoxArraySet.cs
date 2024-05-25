using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxArraySet : LoxCallable
    {
        internal readonly LoxArray array;

        public LoxArraySet(LoxArray array)
        {
            this.array = array;
        }

        public int arity()
        {
            return 2;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            int index = (int)(double)arguments[0];
            Object value = arguments[1];
            return array.elements[index] = value;
        }
    }
}
