package com.craftinginterpreters.lox;

import java.util.List;

class LoxDictSet implements LoxCallable {

   final LoxDict dict;

   public LoxDictSet(LoxDict dict) {
      this.dict = dict;
   }

   @Override
   public int arity() {
      return 2;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      Object key = arguments.get(0);
      Object value = arguments.get(1);
      return dict.elements.put(key, value);
   }
}
