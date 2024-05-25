package com.craftinginterpreters.lox;

import java.util.List;

class LoxLinkedListAdd implements LoxCallable {
   final LoxLinkedList list;
   
   public LoxLinkedListAdd(LoxLinkedList list) {
      this.list = list;
   }
   
   @Override
   public int arity() {
      return 1;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      Object value = arguments.get(0);
      return list.elements.add(value);
   }
}
