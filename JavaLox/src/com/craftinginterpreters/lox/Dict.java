package com.craftinginterpreters.lox;

import java.util.List;

class Dict implements LoxCallable {
   @Override
   public int arity() {
      return 0;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      return new LoxDict();
   }
}
