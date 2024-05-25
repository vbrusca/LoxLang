package com.craftinginterpreters.lox;

import java.util.List;

class LoxDictGet implements LoxCallable {
   final LoxDict dict;
   
   public LoxDictGet(LoxDict dict) {
      this.dict = dict;
   }
   
   @Override
   public int arity() {
      return 1;
   }
   
   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      Object key = arguments.get(0);
      return dict.elements.get(key);
   }
}
