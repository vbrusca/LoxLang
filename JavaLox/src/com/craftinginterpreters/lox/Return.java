package com.craftinginterpreters.lox;

/**
 *
 * @author brusc
 */
class Return extends RuntimeError {
   /**
    * 
    */
   final Object value;

   /**
    * 
    * @param value 
    */
   Return(Object value) {
      super(null, null, false, false);
      this.value = value;
   }
}
