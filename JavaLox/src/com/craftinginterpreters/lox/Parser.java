package com.craftinginterpreters.lox;

import java.util.Arrays;
import java.util.List;
import java.util.ArrayList;

import static com.craftinginterpreters.lox.TokenType.*;

/**
 *
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert
 * Nystrom, Copyright Middlemind Games 05/07/2024
 */
class Parser {

   /**
    *
    */
   public abstract class LoxError extends RuntimeException {

      protected LoxError() {
         super();
      }

      protected LoxError(String message) {
         super(message);
      }

      protected LoxError(String message, Exception innerException) {
         super(message, innerException);
      }
   }

   /**
    *
    */
   private class ParseError extends LoxError {
   }

   /**
    *
    */
   private final List<Token> tokens;

   /**
    *
    */
   private int current = 0;

   /**
    *
    * @param tokens
    */
   Parser(List<Token> tokens) {
      this.tokens = tokens;
   }

   /**
    *
    * @return
    */
   List<Stmt> parse() {
      List<Stmt> statements = new ArrayList<>();
      while (!isAtEnd()) {
         statements.add(declaration());
      }

      return statements;
   }

   /**
    *
    * @return
    */
   private Expr expression() {
      return assignment();
   }

   /**
    *
    * @return
    */
   private Stmt declaration() {
      try {
         if (match(CLASS)) {
            return classDeclaration();
         }

         if (match(FUN)) {
            return function("function");
         }

         if (match(VAR)) {
            return varDeclaration();
         }

         return statement();
      } catch (ParseError error) {
         synchronize();
         return null;
      }
   }

   private Stmt classDeclaration() {
      Token name = consume(IDENTIFIER, "Expect class name.");

      Expr.Variable superclass = null;
      if (match(LESS)) {
         consume(IDENTIFIER, "Expect superclass name.");
         superclass = new Expr.Variable(previous());
      }

      consume(LEFT_BRACE, "Expect '{' before class body.");

      List<Stmt.Function> methods = new ArrayList<>();
      while (!check(RIGHT_BRACE) && !isAtEnd()) {
         methods.add(function("method"));
      }

      consume(RIGHT_BRACE, "Expect '}' after class body.");

      return new Stmt.Class(name, superclass, methods);
   }

   /**
    *
    * @return
    */
   private Stmt statement() {
      if (match(FOR)) {
         return forStatement();
      }
      if (match(IF)) {
         return ifStatement();
      }
      if (match(PRINT)) {
         return printStatement();
      }
      if (match(RETURN)) {
         return returnStatement();
      }
      if (match(WHILE)) {
         return whileStatement();
      }
      if (match(LEFT_BRACE)) {
         return new Stmt.Block(block());
      }

      return expressionStatement();
   }

   /**
    *
    * @return
    */
   private Stmt forStatement() {
      consume(LEFT_PAREN, "Expect '(' after 'for'.");

      Stmt initializer;
      if (match(SEMICOLON)) {
         initializer = null;
      } else if (match(VAR)) {
         initializer = varDeclaration();
      } else {
         initializer = expressionStatement();
      }

      Expr condition = null;
      if (!check(SEMICOLON)) {
         condition = expression();
      }
      consume(SEMICOLON, "Expect ';' after loop condition.");

      Expr increment = null;
      if (!check(RIGHT_PAREN)) {
         increment = expression();
      }
      consume(RIGHT_PAREN, "Expect ')' after for clauses.");
      Stmt body = statement();

      Stmt[] stmts;
      if (increment != null) {
         stmts = new Stmt[]{body, new Stmt.Expression(increment)};
         body = new Stmt.Block(Arrays.asList(stmts));
         //Arrays.asList(body, new Stmt.Expression(increment)));
      }

      if (condition == null) {
         condition = new Expr.Literal(true);
      }
      body = new Stmt.While(condition, body);

      if (initializer != null) {
         stmts = new Stmt[]{initializer, body};
         body = new Stmt.Block(Arrays.asList(stmts));
         //Arrays.asList(initializer, body)
      }

      return body;
   }

   /**
    *
    * @return
    */
   private Stmt ifStatement() {
      consume(LEFT_PAREN, "Expect '(' after 'if'.");
      Expr condition = expression();
      consume(RIGHT_PAREN, "Expect ')' after if condition.");

      Stmt thenBranch = statement();
      Stmt elseBranch = null;
      if (match(ELSE)) {
         elseBranch = statement();
      }

      return new Stmt.If(condition, thenBranch, elseBranch);
   }

   /**
    *
    * @return
    */
   private Stmt printStatement() {
      Expr value = expression();
      consume(SEMICOLON, "Expect ';' after value.");
      return new Stmt.Print(value);
   }

   /**
    *
    * @return
    */
   private Stmt returnStatement() {
      Token keyword = previous();
      Expr value = null;
      if (!check(SEMICOLON)) {
         value = expression();
      }

      consume(SEMICOLON, "Expect ';' after return value.");
      return new Stmt.Return(keyword, value);
   }

   /**
    *
    * @return
    */
   private Stmt varDeclaration() {
      Token name = consume(IDENTIFIER, "Expect variable name.");

      Expr initializer = null;
      if (match(EQUAL)) {
         initializer = expression();
      }

      consume(SEMICOLON, "Expect ';' after variable declaration.");
      return new Stmt.Var(name, initializer);
   }

   /**
    *
    * @return
    */
   private Stmt whileStatement() {
      consume(LEFT_PAREN, "Expect '(' after 'while'.");
      Expr condition = expression();
      consume(RIGHT_PAREN, "Expect ')' after condition.");
      Stmt body = statement();

      return new Stmt.While(condition, body);
   }

   /**
    *
    * @return
    */
   private Stmt expressionStatement() {
      Expr expr = expression();
      consume(SEMICOLON, "Expect ';' after expression.");
      return new Stmt.Expression(expr);
   }

   /**
    *
    * @param kind
    * @return
    */
   private Stmt.Function function(String kind) {
      Token name = consume(IDENTIFIER, "Expect " + kind + " name.");
      consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
      List<Token> parameters = new ArrayList<>();
      if (!check(RIGHT_PAREN)) {
         do {
            if (parameters.size() >= 255) {
               error(peek(), "Can't have more than 255 parameters.");
            }

            parameters.add(
                    consume(IDENTIFIER, "Expect parameter name."));
         } while (match(COMMA));
      }
      consume(RIGHT_PAREN, "Expect ')' after parameters.");

      consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
      List<Stmt> body = block();
      return new Stmt.Function(name, parameters, body);
   }

   /**
    *
    * @return
    */
   private List<Stmt> block() {
      List<Stmt> statements = new ArrayList<>();

      while (!check(RIGHT_BRACE) && !isAtEnd()) {
         statements.add(declaration());
      }

      consume(RIGHT_BRACE, "Expect '}' at the end of a block.");
      return statements;
   }

   /**
    *
    * @return
    */
   private Expr assignment() {
      Expr expr = or();

      if (match(EQUAL)) {
         Token equals = previous();
         Expr value = assignment();

         if (expr instanceof Expr.Variable) {
            Token name = ((Expr.Variable) expr).name;
            return new Expr.Assign(name, value);
         } else if (expr instanceof Expr.Get) {
            Expr.Get get = (Expr.Get) expr;
            return new Expr.Set(get.obj, get.name, value);
         }

         error(equals, "Invalid assignment target.");
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr or() {
      Expr expr = and();

      while (match(OR)) {
         Token oprtr = previous();
         Expr right = and();
         expr = new Expr.Logical(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr and() {
      Expr expr = equality();

      while (match(AND)) {
         Token oprtr = previous();
         Expr right = equality();
         expr = new Expr.Logical(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr equality() {
      Expr expr = comparison();

      while (match(BANG_EQUAL, EQUAL_EQUAL)) {
         Token oprtr = previous();
         Expr right = comparison();
         expr = new Expr.Binary(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr comparison() {
      Expr expr = term();

      while (match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
         Token oprtr = previous();
         Expr right = term();
         expr = new Expr.Binary(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr term() {
      Expr expr = factor();

      while (match(MINUS, PLUS)) {
         Token oprtr = previous();
         Expr right = factor();
         expr = new Expr.Binary(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr factor() {
      Expr expr = unary();

      while (match(SLASH, STAR)) {
         Token oprtr = previous();
         Expr right = unary();
         expr = new Expr.Binary(expr, oprtr, right);
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr unary() {
      if (match(BANG, MINUS)) {
         Token oprtr = previous();
         Expr right = unary();
         return new Expr.Unary(oprtr, right);
      }

      return call();
   }

   /**
    *
    * @param callee
    * @return
    */
   private Expr finishCall(Expr callee) {
      List<Expr> arguments = new ArrayList<>();
      if (!check(RIGHT_PAREN)) {
         do {
            if (arguments.size() >= 255) {
               error(peek(), "Can't have more than 255 arguments.");
            }
            arguments.add(expression());
         } while (match(COMMA));
      }

      Token paren = consume(RIGHT_PAREN, "Expect ')' after arguments.");

      return new Expr.Call(callee, paren, arguments);
   }

   /**
    *
    * @return
    */
   private Expr call() {
      Expr expr = primary();

      while (true) {
         if (match(LEFT_PAREN)) {
            expr = finishCall(expr);
         } else if (match(DOT)) {
            Token name = consume(IDENTIFIER, "Expect property name after '.'.");
            expr = new Expr.Get(expr, name);
         } else {
            break;
         }
      }

      return expr;
   }

   /**
    *
    * @return
    */
   private Expr primary() {
      if (match(FALSE)) {
         //Lox.log(0, "Parser.primary ZZZZ 000");
         return new Expr.Literal(false);
      }
      if (match(TRUE)) {
         //Lox.log(0, "Parser.primary ZZZZ 111");
         return new Expr.Literal(true);
      }

      if (match(NIL)) {
         //Lox.log(0, "Parser.primary ZZZZ 222");
         return new Expr.Literal(null);
      }

      if (match(NUMBER, STRING)) {
         //Lox.log(0, "Parser.primary ZZZZ 333");
         return new Expr.Literal(previous().literal);
      }

      if (match(SUPER)) {
         Token keyword = previous();
         consume(DOT, "Expect '.' after 'super'.");
         Token method = consume(IDENTIFIER, "Expect superclass method name.");
         return new Expr.Super(keyword, method);
      }

      if (match(THIS)) {
         return new Expr.This(previous());
      }

      if (match(IDENTIFIER)) {
         //Lox.log(0, "Parser.primary ZZZZ 444");
         return new Expr.Variable(previous());
      }

      if (match(LEFT_PAREN)) {
         Expr expr = expression();
         consume(RIGHT_PAREN, "Expect ')' after expression.");
         return new Expr.Grouping(expr);
      }

      throw error(peek(), "Expect expression.");
   }

   /**
    *
    * @param types
    * @return
    */
   private boolean match(TokenType... types) {
      for (TokenType type : types) {
         if (check(type)) {
            advance();
            return true;
         }
      }

      return false;
   }

   /**
    *
    * @param type
    * @param message
    * @return
    */
   private Token consume(TokenType type, String message) {
      if (check(type)) {
         return advance();
      }

      throw error(peek(), message);
   }

   /**
    *
    * @param type
    * @return
    */
   private boolean check(TokenType type) {
      if (isAtEnd()) {
         return false;
      }
      return peek().type == type;
   }

   /**
    *
    * @return
    */
   private Token advance() {
      if (!isAtEnd()) {
         current++;
      }
      return previous();
   }

   /**
    *
    * @return
    */
   private boolean isAtEnd() {
      return peek().type == EOF;
   }

   /**
    *
    * @return
    */
   private Token peek() {
      return tokens.get(current);
   }

   /**
    *
    * @return
    */
   private Token previous() {
      return tokens.get(current - 1);
   }

   /**
    *
    * @param token
    * @param message
    * @return
    */
   private ParseError error(Token token, String message) {
      Lox.error(token, message);
      return new ParseError();
   }

   /**
    *
    */
   private void synchronize() {
      advance();

      while (!isAtEnd()) {
         if (previous().type == SEMICOLON) {
            return;
         }

         switch (peek().type) {
            case CLASS:
            case FUN:
            case VAR:
            case FOR:
            case IF:
            case WHILE:
            case PRINT:
            case RETURN:
               return;
         }

         advance();
      }
   }
}
