package com.craftinginterpreters.lox;

import java.util.LinkedList;
import java.util.List;

/**
 *
 * @author brusc
 */
class LoxLinkedList extends LoxInstance {
   
   final List<Object> elements;
   
   LoxLinkedList() {
      super(null);
      elements = new LinkedList<>();      
   }
   
   @Override
   Object get(Token name) {
      if (name.lexeme.equals("get")) {
         return new LoxLinkedListGet(this);
         
      } else if (name.lexeme.equals("set")) {
         return new LoxLinkedListSet(this);
         
      } else if (name.lexeme.equals("add")) {
         return new LoxLinkedListAdd(this);
         
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
      buffer.append("(");
      for (int i = 0; i < elements.size(); i++) {
         if (i != 0) {
            buffer.append(", ");
         }
         buffer.append(elements.get(i));
      }
      buffer.append(")");
      return buffer.toString();
   }   
}
