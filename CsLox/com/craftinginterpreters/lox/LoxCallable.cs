﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal interface LoxCallable
    {
        int arity();
        Object call(Interpreter interpreter, List<Object> arguments);
    }
}
