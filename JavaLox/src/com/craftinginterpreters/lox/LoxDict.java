package com.craftinginterpreters.lox;

import java.util.Map;
import java.util.HashMap;

class LoxDict extends LoxInstance {

   final Map elements;
   
   LoxDict() {
      super(null);
      elements = new HashMap();
   }
   
   @Override
   Object get(Token name) {
      if (name.lexeme.equals("get")) {
         return new LoxDictGet(this);
      } else if (name.lexeme.equals("set")) {
         return new LoxDictSet(this);
      } else if (name.lexeme.equals("length")) {
         return (double) elements.size();
      }

      throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
   }

   @Override
   void set(Token name, Object value) {
      throw new RuntimeError(name, "Can't add properties to arrays.");
   }

   @Override
   public String toString() {
      StringBuilder buffer = new StringBuilder();
      buffer.append("[");
      Object[] keys = elements.keySet().toArray();
      for (int i = 0; i < keys.length; i++) {
         if (i != 0) {
            buffer.append(", ");
         }
         buffer.append(elements.get(keys[i]));
      }
      buffer.append("]");
      return buffer.toString();
   }   
}
