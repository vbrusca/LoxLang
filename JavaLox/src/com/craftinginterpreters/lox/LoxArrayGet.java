package com.craftinginterpreters.lox;

import java.util.List;

class LoxArrayGet implements LoxCallable {
   final LoxArray array;
   
   public LoxArrayGet(LoxArray array) {
      this.array = array;
   }
   
   @Override
   public int arity() {
      return 1;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      int index = (int) (double) arguments.get(0);
      return array.elements[index];
   }
}
