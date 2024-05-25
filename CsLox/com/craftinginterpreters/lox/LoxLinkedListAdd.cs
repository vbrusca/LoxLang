using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LoxLinkedListAdd : LoxCallable
    {
        readonly LoxLinkedList list;

        public LoxLinkedListAdd(LoxLinkedList list)
        {
            this.list = list;
        }

        public int arity()
        {
            return 1;
        }

        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            Object value = arguments[0];
            list.elements.Add(value);
            return (list.elements.Count - 1);
        }
    }
}
