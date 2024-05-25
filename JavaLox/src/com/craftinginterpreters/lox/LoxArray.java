package com.craftinginterpreters.lox;

class LoxArray extends LoxInstance {

   final Object[] elements;

   LoxArray(int size) {
      super(null);
      elements = new Object[size];
   }

   @Override
   Object get(Token name) {
      if (name.lexeme.equals("get")) {
         return new LoxArrayGet(this);
      } else if (name.lexeme.equals("set")) {
         return new LoxArraySet(this);
      } else if (name.lexeme.equals("length")) {
         return (double) elements.length;
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
      for (int i = 0; i < elements.length; i++) {
         if (i != 0) {
            buffer.append(", ");
         }
         buffer.append(elements[i]);
      }
      buffer.append("]");
      return buffer.toString();
   }
}
