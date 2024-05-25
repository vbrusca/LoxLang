package com.craftinginterpreters.lox;

/**
 *
 * @author Victor G. Brusca, Carlo Bruscani, based on the work of Robert Nystrom, Copyright Middlemind Games 05/06/2024
 */
class AstPrinter implements Expr.Visitor<String> {
   /**
    * 
    * @param args 
    */
   public static void main(String[] args) {
      Expr expression = new Expr.Binary(
              new Expr.Unary(
                      new Token(TokenType.MINUS, "-", null, 1),
                      new Expr.Literal(123)),
              new Token(TokenType.STAR, "*", null, 1),
              new Expr.Grouping(
                      new Expr.Literal(45.67)));
      
      System.out.println(new AstPrinter().print(expression));
   }
   
   /**
    * 
    * @param expr
    * @return 
    */
   String print(Expr expr) {
      return expr.accept(this);
   }
   
   /**
    * 
    * @param expr
    * @return 
    */
   @Override
   public String visitBinaryExpr(Expr.Binary expr) {
      return parenthesize(expr.oprtr.lexeme, expr.left, expr.right);
   }
   
   /**
    * 
    * @param expr
    * @return 
    */
   @Override
   public String visitGroupingExpr(Expr.Grouping expr) {
      return parenthesize("group", expr.expression);
   }
   
   /**
    * 
    * @param expr
    * @return 
    */
   @Override
   public String visitLiteralExpr(Expr.Literal expr) {
      if(expr.value == null) return "nil";
      return expr.value.toString();
   }
   
   /**
    * 
    * @param expr
    * @return 
    */
   @Override
   public String visitUnaryExpr(Expr.Unary expr) {
      return parenthesize(expr.oprtr.lexeme, expr.right);
   }
   
   /**
    * 
    * @param name
    * @param expr
    * @return 
    */
   private String parenthesize(String name, Expr expr) {
      return parenthesize(name, new Expr[] {expr});
   }   
   
   /**
    * 
    * @param name
    * @param exprs
    * @return 
    */
   private String parenthesize(String name, Expr... exprs) {
      StringBuilder builder = new StringBuilder();
      
      builder.append("(").append(name);
      for(Expr expr : exprs) {
         builder.append(" ");
         builder.append(expr.accept(this));
      }
      builder.append(")");
      
      return builder.toString();
   }

   /**
    * 
    * @param expr
    * @return 
    */
   @Override
   public String visitVariableExpr(Expr.Variable expr) {
      throw new UnsupportedOperationException("Not supported yet."); // Generated from nbfs://nbhost/SystemFileSystem/Templates/Classes/Code/GeneratedMethodBody
   }
}