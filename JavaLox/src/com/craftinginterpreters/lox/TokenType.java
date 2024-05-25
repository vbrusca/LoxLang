package com.craftinginterpreters.lox;

/**
 * An enumeration the represents some key language token types.
 * 
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert Nystrom, Copyright Middlemind Games 05/03/2024
 */
public enum TokenType {
   // Single-character tokens.
   LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
   COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

   // One or two character tokens.
   BANG, BANG_EQUAL, 
   EQUAL, EQUAL_EQUAL, 
   GREATER, GREATER_EQUAL, 
   LESS, LESS_EQUAL,

   // Literals.
   IDENTIFIER, STRING, NUMBER,

   // Keywords.
   AND, CLASS, ELSE, FALSE, FUN, FOR, IF, NIL, OR,
   PRINT, RETURN, SUPER, THIS, TRUE, VAR, WHILE,

   EOF
}
