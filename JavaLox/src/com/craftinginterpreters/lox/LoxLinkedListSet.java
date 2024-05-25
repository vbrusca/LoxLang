package com.craftinginterpreters.lox;

import java.util.List;

class LoxLinkedListSet implements LoxCallable {
   final LoxLinkedList list;
   
   public LoxLinkedListSet(LoxLinkedList list) {
      this.list = list;
   }
   
   @Override
   public int arity() {
      return 2;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      int index = (int) (double) arguments.get(0);
      Object value = arguments.get(1);
      Object ret = null;
      if(index >= 0 && index < list.elements.size()) {
         ret = list.elements.set(index, value);
      }
      return ret;
   }
}
