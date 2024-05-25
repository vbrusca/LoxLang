package com.craftinginterpreters.lox;

import java.util.List;

class LoxArraySet implements LoxCallable {
   final LoxArray array;

   public LoxArraySet(LoxArray array) {
      this.array = array;
   }

   @Override
   public int arity() {
      return 2;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      int index = (int) (double) arguments.get(0);
      Object value = arguments.get(1);
      return array.elements[index] = value;
   }
}
