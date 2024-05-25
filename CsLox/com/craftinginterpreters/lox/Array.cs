using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class Array : LoxCallable
    {
        public int arity()
        {
            return 1;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            int size = (int)(double)arguments[0];
            return new LoxArray(size);
        }
    }
}
