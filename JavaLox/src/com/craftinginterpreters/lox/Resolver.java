package com.craftinginterpreters.lox;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Stack;

/**
 *
 * @author brusc
 */
class Resolver implements Expr.Visitor<Object>, Stmt.Visitor<Object> {

   private final Interpreter interpreter;
   private final Stack<Map<String, Boolean>> scopes = new Stack<>();
   private FunctionType currentFunction = FunctionType.NONE;
   private ClassType currentClass = ClassType.NONE;

   private enum FunctionType {
      NONE,
      FUNCTION,
      INITIALIZER,
      METHOD
   }

   private enum ClassType {
      NONE,
      CLASS,
      SUBCLASS
   }

   Resolver(Interpreter interpreter) {
      this.interpreter = interpreter;
   }

   void resolve(List<Stmt> statements) {
      for (Stmt statement : statements) {
         resolve(statement);
      }
   }

   private void resolve(Stmt stmt) {
      stmt.accept(this);
   }

   private void resolve(Expr expr) {
      expr.accept(this);
   }

   private void resolveFunction(Stmt.Function function, FunctionType type) {
      FunctionType enclosingFunction = currentFunction;
      currentFunction = type;

      beginScope();
      for (Token param : function.prms) {
         declare(param);
         define(param);
      }
      resolve(function.body);
      endScope();
      currentFunction = enclosingFunction;
   }

   private void beginScope() {
      //Lox.log(0, "Resolver.beginScope AAA " + scopes.size());
      scopes.push(new HashMap<String, Boolean>());
      //Lox.log(0, "Resolver.beginScope BBB " + scopes.size());
   }

   private void endScope() {
      //Lox.log(0, "Resolver.endScope AAA " + scopes.size());
      scopes.pop();
      //Lox.log(0, "Resolver.endScope BBB " + scopes.size());
   }

   private void declare(Token name) {
      //Lox.log(0, "Resolver.declare " + name.lexeme + " scopes.size = " + scopes.size());
      if (scopes.size() == 0) {
         return;
      }

      Map<String, Boolean> scope = scopes.peek();
      if (scope.containsKey(name.lexeme)) {
         Lox.error(name, "Already a variable with this name in this scope.");
      }

      scope.put(name.lexeme, false);
      //Lox.log(0, "Resolver.define scope[" + name.lexeme + "] = false");
   }

   private void define(Token name) {
      //Lox.log(0, "Resolver.define " + name.lexeme + " scopes.size = " + scopes.size());
      if (scopes.size() == 0) {
         return;
      }

      Map<String, Boolean> scope = scopes.peek();
      scope.put(name.lexeme, true);
      //Lox.log(0, "Resolver.define scope[" + name.lexeme + "] = true");
   }

   private void resolveLocal(Expr expr, Token name) {
      /*
      Lox.log(0, "Resolver.resolveLocal scopes.size = " + scopes.size());
      Lox.log(0, "Resolver.resolveLocal expr = '" + expr + "', name = '" + name.literal + "'");
      for (int z = scopes.size() - 1; z >= 0; z--)
      {
          Lox.log(0, "Resolver.resolveLocal YYY index = " + z + ", size = " + scopes.get(z).keySet().size());         
          for (int j = 0; j < scopes.get(z).keySet().size(); j++)
          {
              Lox.log(0, "Resolver.resolveLocal YYY key = " + scopes.get(z).keySet().toArray()[j]);
          }
      }
       */
      for (int i = scopes.size() - 1; i >= 0; i--) {
         //Lox.log(0, "Resolver.resolveLocal AAA index = " + i + ", size = " + scopes.get(i).size() + " looking for = '" + name.lexeme + "'");
         if (scopes.get(i).containsKey(name.lexeme)) {
            //Lox.log(0, "Resolver.resolveLocal BBB found = '" + name.lexeme + "', expr = '" + expr + "', distance = '" + (scopes.size() - 1 - i) + "', " + i);
            interpreter.resolve(expr, scopes.size() - 1 - i);
            return;
         }
      }
   }

   @Override
   public Object visitBlockStmt(Stmt.Block stmt) {
      beginScope();
      resolve(stmt.statements);
      endScope();
      return null;
   }

   @Override
   public Object visitClassStmt(Stmt.Class stmt) {
      ClassType enclosingClass = currentClass;
      currentClass = ClassType.CLASS;

      declare(stmt.name);
      define(stmt.name);

      if (stmt.superclass != null && stmt.name.lexeme.equals(stmt.superclass.name.lexeme)) {
         Lox.error(stmt.superclass.name, "A class can't inherit from itself.");
      }

      if (stmt.superclass != null) {
         currentClass = ClassType.SUBCLASS;
         resolve(stmt.superclass);
      }

      if (stmt.superclass != null) {
         beginScope();
         scopes.peek().put("super", true);
      }

      beginScope();
      scopes.peek().put("this", true);

      for (Stmt.Function method : stmt.methods) {
         FunctionType declaration = FunctionType.METHOD;
         if (method.name.lexeme.equals("init")) {
            declaration = FunctionType.INITIALIZER;
         }

         resolveFunction(method, declaration);
      }

      endScope();

      if (stmt.superclass != null) {
         endScope();
      }

      currentClass = enclosingClass;
      return null;
   }

   @Override
   public Object visitExpressionStmt(Stmt.Expression stmt) {
      resolve(stmt.expression);
      return null;
   }

   @Override
   public Object visitFunctionStmt(Stmt.Function stmt) {
      declare(stmt.name);
      define(stmt.name);

      resolveFunction(stmt, FunctionType.FUNCTION);
      return null;
   }

   @Override
   public Object visitIfStmt(Stmt.If stmt) {
      resolve(stmt.condition);
      resolve(stmt.thenBranch);
      if (stmt.elseBranch != null) {
         resolve(stmt.elseBranch);
      }
      return null;
   }

   @Override
   public Object visitPrintStmt(Stmt.Print stmt) {
      resolve(stmt.expression);
      return null;
   }

   @Override
   public Object visitReturnStmt(Stmt.Return stmt) {
      if (currentFunction == FunctionType.NONE) {
         Lox.error(stmt.keyword, "Can't return from top-level code.");
      }

      if (stmt.value != null) {
         if (currentFunction == FunctionType.INITIALIZER) {
            Lox.error(stmt.keyword, "Can't return a value from an initializer.");
         }

         resolve(stmt.value);
      }

      return null;
   }

   @Override
   public Object visitVarStmt(Stmt.Var stmt) {
      declare(stmt.name);
      if (stmt.initializer != null) {
         resolve(stmt.initializer);
      }
      define(stmt.name);
      return null;
   }

   @Override
   public Object visitWhileStmt(Stmt.While stmt) {
      resolve(stmt.condition);
      resolve(stmt.body);
      return null;
   }

   @Override
   public Object visitVariableExpr(Expr.Variable expr) {
      if (scopes.size() != 0 && scopes.peek().containsKey(expr.name.lexeme) && scopes.peek().get(expr.name.lexeme) == Boolean.FALSE) {
         Lox.error(expr.name, "Can't read local variable in its own initializer.");
      }

      resolveLocal(expr, expr.name);
      return null;
   }

   @Override
   public Object visitAssignExpr(Expr.Assign expr) {
      resolve(expr.value);
      resolveLocal(expr, expr.name);
      return null;
   }

   @Override
   public Object visitBinaryExpr(Expr.Binary expr) {
      resolve(expr.left);
      resolve(expr.right);
      return null;
   }

   @Override
   public Object visitCallExpr(Expr.Call expr) {
      resolve(expr.callee);

      for (Expr argument : expr.arguments) {
         resolve(argument);
      }

      return null;
   }

   @Override
   public Object visitGetExpr(Expr.Get expr) {
      resolve(expr.obj);
      return null;
   }

   @Override
   public Object visitGroupingExpr(Expr.Grouping expr) {
      resolve(expr.expression);
      return null;
   }

   @Override
   public Object visitLiteralExpr(Expr.Literal expr) {
      return null;
   }

   @Override
   public Object visitLogicalExpr(Expr.Logical expr) {
      resolve(expr.left);
      resolve(expr.right);
      return null;
   }

   @Override
   public Object visitSetExpr(Expr.Set expr) {
      resolve(expr.value);
      resolve(expr.obj);
      return null;
   }

   @Override
   public Object visitSuperExpr(Expr.Super expr) {
      if (currentClass == ClassType.NONE) {
         Lox.error(expr.keyword,
                 "Can't use 'super' outside of a class.");
      } else if (currentClass != ClassType.SUBCLASS) {
         Lox.error(expr.keyword,
                 "Can't use 'super' in a class with no superclass.");
      }

      resolveLocal(expr, expr.keyword);
      return null;
   }

   @Override
   public Object visitThisExpr(Expr.This expr) {
      if (currentClass == ClassType.NONE) {
         Lox.error(expr.keyword, "Can't use 'this' outside of a class.");
         return null;
      }

      resolveLocal(expr, expr.keyword);
      return null;
   }

   @Override
   public Object visitUnaryExpr(Expr.Unary expr) {
      resolve(expr.right);
      return null;
   }
}
