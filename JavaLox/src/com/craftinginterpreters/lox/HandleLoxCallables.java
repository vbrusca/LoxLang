package com.craftinginterpreters.lox;

import java.util.List;

/**
 *
 * @author brusc
 */
interface HandleLoxCallables {
   public Object call(int arity, Interpreter interpreter, List<Object> arguments);
}
