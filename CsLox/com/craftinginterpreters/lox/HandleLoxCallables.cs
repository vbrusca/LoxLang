using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal interface HandleLoxCallables
    {
        Object call(int arity, Interpreter interpreter, List<Object> arguments);
    }
}
