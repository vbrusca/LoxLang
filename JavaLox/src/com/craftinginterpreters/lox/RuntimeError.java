package com.craftinginterpreters.lox;

/**
 *
 * @author brusc
 */
class RuntimeError extends RuntimeException {
   /**
    * 
    */
   final Token token;
   
   /**
    * 
    * @param token
    * @param message 
    */
   RuntimeError(Token token, String message) {
      super(message);
      this.token = token;
   }

   /**
    * 
    * @param message
    * @param cause
    * @param enableSuppression
    * @param writableStackTrace 
    */
   RuntimeError(String message, Object cause, boolean enableSuppression, boolean writableStackTrace) {
      super(message, (Throwable)cause, enableSuppression, writableStackTrace);
      token = null;
   }
}
