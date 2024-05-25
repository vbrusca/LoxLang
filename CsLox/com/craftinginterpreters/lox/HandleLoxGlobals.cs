using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal interface HandleLoxGlobals
    {
        void defineGlobals(Environment globals);
    }
}
