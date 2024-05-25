package com.craftinginterpreters.lox;

import java.util.List;

class LoxLinkedListGet implements LoxCallable {
   final LoxLinkedList list;
   
   public LoxLinkedListGet(LoxLinkedList list) {
      this.list = list;
   }
   
   @Override
   public int arity() {
      return 1;
   }
   
   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      int index = (int) (double) arguments.get(0);
      return list.elements.get(index);
   }
}
