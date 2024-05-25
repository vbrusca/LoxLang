using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxLinkedListSet : LoxCallable
    {
        readonly LoxLinkedList list;

        public LoxLinkedListSet(LoxLinkedList list)
        {
            this.list = list;
        }

        public int arity()
        {
            return 2;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            int index = (int)(double)arguments[0];
            Object value = arguments[1];
            Object ret = null;
            if (index >= 0 && index < list.elements.Count)
            {
                ret = list.elements[index];
                list.elements[index] = value;
            }
            return ret;
        }
    }
}
