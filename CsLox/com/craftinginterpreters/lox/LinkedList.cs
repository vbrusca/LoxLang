using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class LinkedList : LoxCallable
    {
        public virtual int arity()
        {
            return 0;
        }

        public virtual object call(Interpreter interpreter, List<object> arguments)
        {
            return new LoxLinkedList();
        }
    }
}
