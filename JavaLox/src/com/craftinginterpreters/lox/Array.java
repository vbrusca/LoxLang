package com.craftinginterpreters.lox;

import java.util.List;

/**
 *
 * @author brusc
 */
class Array implements LoxCallable {
   @Override
   public int arity() {
      return 1;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      int size = (int) (double) arguments.get(0);
      return new LoxArray(size);
   }
}
