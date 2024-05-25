package com.craftinginterpreters.lox;

/**
 * A class that represents an identified token in the Lox scripting language.
 * 
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert Nystrom, Copyright Middlemind Games 05/03/2024
 */
class Token {
   /**
    * 
    */
   final TokenType type;
   
   /**
    * 
    */
   final String lexeme;

   /**
    * 
    */
   final Object literal;
   
   /**
    * 
    */
   final int line;

   /**
    * 
    * 
    * @param type
    * @param lexeme
    * @param literal
    * @param line 
    */
   Token(TokenType type, String lexeme, Object literal, int line) {
      this.type = type;
      this.lexeme = lexeme;
      this.literal = literal;
      this.line = line;
   }
   
   /**
    * 
    * @return 
    */
   @Override
   public String toString() {
      return type + " " + lexeme + " " + literal;
   }
}
