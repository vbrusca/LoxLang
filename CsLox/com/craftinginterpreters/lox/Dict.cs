using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class Dict : LoxCallable
    {
        public virtual int arity()
        {
            return 0;
        }

        public virtual Object call(Interpreter interpreter, List<Object> arguments)
        {
            return new LoxDict();
        }
    }
}
