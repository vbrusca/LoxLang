package com.craftinginterpreters.lox;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import static com.craftinginterpreters.lox.TokenType.*;

/**
 * 
 * 
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert Nystrom, Copyright Middlemind Games 05/03/2024
 */
class Scanner {
   /**
    * 
    */
   private static final Map<String, TokenType> keywords;
   
   static {
      keywords = new HashMap<>();
      keywords.put("and", AND);
      keywords.put("class", CLASS);
      keywords.put("else", ELSE);
      keywords.put("false", FALSE);
      keywords.put("for", FOR);
      keywords.put("fun", FUN);
      keywords.put("if", IF);
      keywords.put("nil", NIL);
      keywords.put("null", NIL);      
      keywords.put("or", OR);
      keywords.put("print", PRINT);
      keywords.put("return", RETURN);
      keywords.put("super", SUPER);
      keywords.put("this", THIS);
      keywords.put("true", TRUE);
      keywords.put("var", VAR);
      keywords.put("while", WHILE);
   }

   /**
    * 
    */
   private final String source;
   
   /**
    * 
    */
   private final List<Token> tokens = new ArrayList<>();
   
   /**
    * 
    */
   private int start = 0;
   
   /**
    * 
    */
   private int current = 0;
   
   /**
    * 
    */
   private int line = 0;
   
   /**
    * 
    * @param source 
    */
   Scanner(String source) {
      this.source = source;
   }
   
   /**
    * 
    * @return 
    */
   List<Token> scanTokens() {
      while(!isAtEnd()) {
         // We are at the beginning of the next lexeme.
         start = current;
         scanToken();
      }
      
      tokens.add(new Token(EOF, "", null, line));
      return tokens;
   }
   
   /**
    * 
    */
   private void scanToken() {
      char c = advance();
      switch(c) {
         case '(': addToken(LEFT_PAREN); break;
         case ')': addToken(RIGHT_PAREN); break;
         case '{': addToken(LEFT_BRACE); break;
         case '}': addToken(RIGHT_BRACE); break;
         case ',': addToken(COMMA); break;
         case '.': addToken(DOT); break;
         case '-': addToken(MINUS); break;
         case '+': addToken(PLUS); break;
         case ';': addToken(SEMICOLON); break;
         case '*': addToken(STAR); break;
         case '!':
            addToken(match('=') ? BANG_EQUAL : BANG);
            break;
         case '=':
            addToken(match('=') ? EQUAL_EQUAL : EQUAL);
            break;
         case '<':
            addToken(match('=') ? LESS_EQUAL : LESS);
            break;
         case '>':
            addToken(match('=') ? GREATER_EQUAL : GREATER);
            break;
         case '/':
            if(match('/')) {
               // A comment goes until the end of the line.
               while(peek() != '\n' && !isAtEnd()) advance();
            } else if(match('*')) {
               // A multi-line comment goes until a '*/' is encountered
               // Doesn't support multi-line comments inside a multi-line comment
               //while ((peek() != '*' && peekNext() != '/') && !isAtEnd()) advance();
               
               // Supports multi-line comments inside a multi-line comment               
               int stillOpened = 1;
               while(stillOpened > 0 && !isAtEnd()) {
                  if(peek() == '/' && peekNext() == '*') {
                     stillOpened++;
                     advance();
                     advance();
                  } else if(peek() == '*' && peekNext() == '/') {
                     stillOpened--;
                     advance();
                     advance();                     
                  } else {
                     advance();
                  }
               }
               
               if(stillOpened != 0) Lox.error(line, "Expected ending multiline comment '*/'.");               
            } else {
               addToken(SLASH);
            }
            break;
            
         case ' ':
         case '\r':
         case '\t':
            // Ignore whitespace.
            break;
            
         case '\n':
            line++;
            break;
            
         case '"': loadString(); break;
            
         default:
            if(isDigit(c)) {
               loadNumber();
            } else if(isAlpha(c)) {
               identifier();
            } else {
               Lox.error(line, "Unexpected character.");
            }
            break;
      }
   }
   
   /**
    * 
    * @return 
    */
   private boolean isAtEnd() {
      return current >= source.length();
   }
   
   /**
    * 
    * @return 
    */
   private char advance() {
      return source.charAt(current++);
   }
   
   /**
    * 
    * @param type 
    */
   private void addToken(TokenType type) {
      addToken(type, null);
   }

   /**
    * 
    * @param type
    * @param literal 
    */
   private void addToken(TokenType type, Object literal) {
      int s = start;
      int e = current;
      String text = source.substring(s, e);
      //Lox.log(line, "(addToken) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
      tokens.add(new Token(type, text, literal, line));
   }
   
   /**
    * 
    */
   private void identifier() {
      while(isAlphaNumeric(peek())) advance();
      
      int s = start;
      int e = current;      
      String text = source.substring(s, e);
      //Lox.log(line, "(identifier) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
      TokenType type = keywords.get(text);
      if(type == null) type = IDENTIFIER;
      addToken(type);
   }
   
   /**
    * 
    */
   private void loadNumber() {
      while(isDigit(peek())) advance();
      
      // Look for a fractional part.
      if(peek() == '.' && isDigit(peekNext())) {
         // Consume the "."
         advance();
         
         while(isDigit(peek())) advance();
      }

      int s = start;
      int e = current;      
      String text = source.substring(s, e);
      //Lox.log(line, "(loadNumber) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
      addToken(NUMBER, Double.parseDouble(text));
   }
   
   /**
    * 
    */
   private void loadString() {
      while(peek() != '"' && !isAtEnd()) {
         if(peek() == '\n') line++;
         advance();
      }
      
      if(isAtEnd()) {
         Lox.error(line, "Unterminated string.");
         return;
      }
      
      // The closing ".
      advance();
      
      // Trim the surrounding quotes.
      // Handle unescape sequences here.
      int s = start + 1;
      int e = current - 1;      
      String value = source.substring(s, e);
      //Lox.log(line, "(loadString) Found text '" + value + "' with substring offset start '" + s + "' and end '" + e + "'.");
      addToken(STRING, value);
   }
   
   /**
    * 
    * @param expected
    * @return 
    */
   private boolean match(char expected) {
      if(isAtEnd()) return false;
      if(source.charAt(current) != expected) return false;
      
      current++;
      return true;
   }
   
   /**
    * 
    * @return 
    */
   private char peek() {
      if(isAtEnd()) return '\0';
      return source.charAt(current);
   }
   
   /**
    * 
    * @return 
    */
   private char peekNext() {
      if(current + 1 >= source.length()) return '\0';
      return source.charAt(current + 1);
   }
   
   /**
    * 
    * @param c
    * @return 
    */
   private boolean isAlpha(char c) {
      return ((c >= 'a' && c <= 'z') ||
              (c >= 'A' && c <= 'Z') ||
              c == '_');
   }
   
   /**
    * 
    * @param c
    * @return 
    */
   private boolean isAlphaNumeric(char c) {
      return (isAlpha(c) || isDigit(c));
   }
   
   /**
    * 
    * @param c
    * @return 
    */
   private boolean isDigit(char c) {
      return c >= '0' && c <= '9';
   }
}