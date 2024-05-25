using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxLinkedListGet : LoxCallable
    {
        readonly LoxLinkedList list;

        public LoxLinkedListGet(LoxLinkedList list)
        {
            this.list = list;
        }

        public int arity()
        {
            return 1;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            int index = (int)(double)arguments[0];
            return list.elements[index];
        }
    }
}
