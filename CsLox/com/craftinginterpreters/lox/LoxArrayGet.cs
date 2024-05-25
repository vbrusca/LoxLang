using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxArrayGet : LoxCallable
    {
        readonly LoxArray array;

        public LoxArrayGet(LoxArray array)
        {
            this.array = array;
        }

        public int arity()
        {
            return 1;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            int index = (int)(double)arguments[0];
            return array.elements[index];
        }
    }
}
