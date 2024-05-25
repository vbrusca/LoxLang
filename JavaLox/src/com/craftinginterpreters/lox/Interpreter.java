package com.craftinginterpreters.lox;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 *
 * @author brusc
 */
@SuppressWarnings("NonPublicExported")
public class Interpreter implements Expr.Visitor<Object>, Stmt.Visitor<Object> {

   final Environment globals = new Environment();
   final Map<Expr, Integer> locals = new HashMap<>();

   private Environment environment;
   public Map<Object, HandleLoxCallables> handleCalls;
   public ExternalLoxCallable externalFunctions;
   public HandleLoxGlobals externalGlobals;
   public int currentLine = 0;

   public Interpreter() {
      this.handleCalls = new HashMap<>();
      this.externalGlobals = null;
      initialize();
   }

   public Interpreter(HashMap<Object, HandleLoxCallables> handleCalls) {
      this.handleCalls = handleCalls;
      this.externalGlobals = null;
      initialize();
   }

   public Interpreter(HashMap<Object, HandleLoxCallables> handleCalls, HandleLoxGlobals externalGlobals) {
      this.handleCalls = handleCalls;
      this.externalGlobals = externalGlobals;
      initialize();
   }

   private void initialize() {
      environment = globals;
      globals.define("clock", new Clock());
      globals.define("Array", new Array());
      globals.define("List", new LinkedList());
      globals.define("Dict", new Dict());      

      externalFunctions = new ExternalLoxCallable(handleCalls);
      globals.define("sys", externalFunctions);
      globals.define("GBL_NUM_MIN", Double.MAX_VALUE * -1.0);       //-1.7976931348623157E+308
      globals.define("GBL_NUM_MAX", Double.MAX_VALUE);              // 1.7976931348623157E+308

      if (externalGlobals != null) {
         externalGlobals.defineGlobals(globals);
      }
   }

   /**
    *
    * @param statements
    */
   void interpret(List<Stmt> statements) {
      currentLine = 0;
      try {
         for (Stmt statement : statements) {
            execute(statement);
            currentLine++;
         }
      } catch (RuntimeError error) {
         Lox.runtimeError(error);
      }
   }

   /**
    *
    * @param expr
    * @return
    */
   private Object evaluate(Expr expr) {
      return expr.accept(this);
   }

   /**
    *
    * @param stmt
    */
   private void execute(Stmt stmt) {
      stmt.accept(this);
   }

   void resolve(Expr expr, int depth) {
      //Lox.log(0, "Interpreter.resolve expr = '" + expr.getClass() + "', depth = '" + depth + "'");
      locals.put(expr, depth);
   }

   /**
    *
    * @param statements
    * @param environment
    */
   void executeBlock(List<Stmt> statements, Environment environment) {
      Environment previous = this.environment;
      try {
         this.environment = environment;

         for (Stmt statement : statements) {
            execute(statement);
         }
      } finally {
         this.environment = previous;
      }
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitBinaryExpr(Expr.Binary expr) {
      //Lox.log(0, "Interpreter.visitBinaryExpr: left = '" + expr.left + "' right = '" + expr.right + "', oprtr = '" + expr.oprtr + "'");
      Object left = evaluate(expr.left);
      Object right = evaluate(expr.right);

      switch (expr.oprtr.type) {
         case BANG_EQUAL:
            return !isEqual(left, right);
         case EQUAL_EQUAL:
            return isEqual(left, right);
         case GREATER:

            if ((right instanceof Double) && left == null) {
               return 0.0 > (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left > 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left > (double) right;
         case GREATER_EQUAL:

            if ((right instanceof Double) && left == null) {
               return 0.0 >= (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left >= 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left >= (double) right;
         case LESS:

            if ((right instanceof Double) && left == null) {
               return 0.0 < (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left < 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left < (double) right;
         case LESS_EQUAL:

            if ((right instanceof Double) && left == null) {
               return 0.0 <= (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left <= 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left <= (double) right;
         case MINUS:

            if ((right instanceof Double) && left == null) {
               return 0.0 - (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left - 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left - (double) right;
         case PLUS:
            /*
            if (left != null) {
               Lox.log(0, "Left type: " + left.getClass());
            }
            if (right != null) {
               Lox.log(0, "Right type: " + right.getClass());
            }
            Lox.log(0, "Arg Types: left = '" + left + "', right = '" + right + "'");
             */

            if (right instanceof Double && left == null) {
               return 0.0 + (double) right;
            }

            if (left instanceof Double && right == null) {
               return (double) left + 0.0;
            }

            if (left instanceof Double && right instanceof Double) {
               return (double) left + (double) right;
            }

            if (right instanceof String && left == null) {
               return "" + (String) right;
            }

            if (left instanceof String && right == null) {
               return (String) left + "";
            }

            if (left instanceof String && right instanceof String) {
               return (String) left + (String) right;
            }

            throw new RuntimeError(expr.oprtr, "Operands must be two numbers or two strings.");
         case SLASH:

            if (right instanceof String && left == null) {
               return 0.0 / (Double) right;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left / (double) right;
         case STAR:

            if ((right instanceof Double) && left == null) {
               return 0.0 * (double) right;
            }

            if ((left instanceof Double) && right == null) {
               return (double) left * 0.0;
            }

            checkNumberOperands(expr.oprtr, left, right);
            return (double) left * (double) right;
      }

      // Unreachable.
      Lox.log(currentLine, "Warning! About to return null from the Interpeter's visitBinaryExpr method.");
      return null;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitGroupingExpr(Expr.Grouping expr) {
      return evaluate(expr.expression);
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitLiteralExpr(Expr.Literal expr) {
      //if (expr.value == null) {
      //Lox.log(0, "Interpreter.visitLiteralExpr expr.value is null");
      //}
      //Lox.log(0, "Interpreter.visitLiteralExpr expr.value = '" + expr.value + "'");
      return expr.value;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitUnaryExpr(Expr.Unary expr) {
      Object right = evaluate(expr.right);

      switch (expr.oprtr.type) {
         case BANG:
            return !isTruthy(right);
         case MINUS:
            checkNumberOperand(expr.oprtr, right);
            return -(double) right;
      }

      // Unreachable.
      Lox.log(currentLine, "Warning! About to return null from the Interpeter's visitUnaryExpr method.");
      return null;
   }

   /**
    *
    * @param oprtr
    * @param operand
    */
   private void checkNumberOperand(Token oprtr, Object operand) {
      if (operand instanceof Double) {
         return;
      }
      throw new RuntimeError(oprtr, "Operand must be a number.");
   }

   /**
    *
    * @param oprtr
    * @param left
    * @param right
    */
   private void checkNumberOperands(Token oprtr, Object left, Object right) {
      if (left instanceof Double && right instanceof Double) {
         return;
      }

      throw new RuntimeError(oprtr, "Operands must be numbers.");
   }

   /**
    *
    * @param obj
    * @return
    */
   private boolean isTruthy(Object obj) {
      if (obj == null) {
         return false;
      }
      if (obj instanceof Boolean) {
         return (boolean) obj;
      }
      return true;
   }

   /**
    *
    * @param a
    * @param b
    * @return
    */
   private boolean isEqual(Object a, Object b) {
      if (a == null && b == null) {
         return true;
      }
      if (a == null) {
         return false;
      }

      return a.equals(b);
   }

   /**
    *
    * @param object
    * @return
    */
   private String stringify(Object obj) {
      if (obj == null) {
         return "nil";
      }

      if (obj instanceof Double) {
         String text = obj.toString();
         if (text.endsWith(".0")) {
            text = text.substring(0, text.length() - 2);
         }
         return text;
      }

      return obj.toString();
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitExpressionStmt(Stmt.Expression stmt) {
      evaluate(stmt.expression);
      return null;
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitPrintStmt(Stmt.Print stmt) {
      Object value = evaluate(stmt.expression);
      String str = stringify(value);
      System.out.println(str);
      return null;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitVariableExpr(Expr.Variable expr) {
      //Lox.log(0, "visitVariableExpr: '" + expr.name.lexeme + "'");
      return lookUpVariable(expr.name, expr); //environment.get(expr.name);
   }

   private Object lookUpVariable(Token name, Expr expr) {
      //Lox.log(0, "LookUpVariable: '" + name.lexeme + "': " + expr);
      Integer distance = Integer.MIN_VALUE;
      if (locals.containsKey(expr)) {
         distance = locals.get(expr);
      }

      if (distance != Integer.MIN_VALUE) {
         //Lox.log(0, "LookUpVariable: 'AAA'");
         return environment.getAt(distance, name.lexeme);
      } else {
         //Lox.log(0, "LookUpVariable: 'BBB'");
         return globals.get(name);
      }
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitVarStmt(Stmt.Var stmt) {
      Object value = null;
      if (stmt.initializer != null) {
         value = evaluate(stmt.initializer);
      }

      //Lox.log(0, "Interpreter.visitVarStmt: value = '" + value + "', init: '" + stmt.name + "', " + stmt.initializer);
      environment.define(stmt.name.lexeme, value);
      return null;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitAssignExpr(Expr.Assign expr) {
      Object value = evaluate(expr.value);

      Integer distance = Integer.MIN_VALUE;
      if (locals.containsKey(expr)) {
         distance = locals.get(expr);
      }

      if (distance != Integer.MIN_VALUE) {
         environment.assignAt(distance, expr.name, value);
      } else {
         globals.assign(expr.name, value);
      }

      return value;
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitBlockStmt(Stmt.Block stmt) {
      executeBlock(stmt.statements, new Environment(environment));
      return null;
   }

   @Override
   public Object visitClassStmt(Stmt.Class stmt) {
      Object superclass = null;
      if (stmt.superclass != null) {
         superclass = evaluate(stmt.superclass);
         if (!(superclass instanceof LoxClass)) {
            throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
         }
      }

      environment.define(stmt.name.lexeme, null);

      if (stmt.superclass != null) {
         environment = new Environment(environment);
         environment.define("super", superclass);
      }

      Map<String, LoxFunction> methods = new HashMap<>();
      for (Stmt.Function method : stmt.methods) {
         LoxFunction function = new LoxFunction(method, environment, (method.name.lexeme.equals("init")));
         methods.put(method.name.lexeme, function);
      }

      LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass) superclass, methods);

      if (superclass != null) {
         environment = environment.enclosing;
      }

      environment.assign(stmt.name, klass);
      return null;
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitIfStmt(Stmt.If stmt) {
      if (isTruthy(evaluate(stmt.condition))) {
         execute(stmt.thenBranch);
      } else if (stmt.elseBranch != null) {
         execute(stmt.elseBranch);
      }
      return null;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitLogicalExpr(Expr.Logical expr) {
      Object left = evaluate(expr.left);

      if (expr.oprtr.type == TokenType.OR) {
         if (isTruthy(left)) {
            return left;
         }
      } else {
         if (!isTruthy(left)) {
            return left;
         }
      }

      return evaluate(expr.right);
   }

   @Override
   public Object visitSetExpr(Expr.Set expr) {
      Object obj = evaluate(expr.obj);

      if (!(obj instanceof LoxInstance)) {
         throw new RuntimeError(expr.name, "Only instances have fields.");
      }

      Object value = evaluate(expr.value);
      ((LoxInstance) obj).set(expr.name, value);
      return value;
   }

   @Override
   public Object visitSuperExpr(Expr.Super expr) {
      int distance = locals.get(expr);
      LoxClass superclass = (LoxClass) environment.getAt(distance, "super");

      LoxInstance obj = (LoxInstance) environment.getAt(distance - 1, "this");

      LoxFunction method = superclass.findMethod(expr.method.lexeme);

      if (method == null) {
         throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
      }

      return method.bind(obj);
   }

   @Override
   public Object visitThisExpr(Expr.This expr) {
      return lookUpVariable(expr.keyword, expr);
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitWhileStmt(Stmt.While stmt) {
      while (isTruthy(evaluate(stmt.condition))) {
         execute(stmt.body);
      }
      return null;
   }

   /**
    *
    * @param expr
    * @return
    */
   @Override
   public Object visitCallExpr(Expr.Call expr) {
      Object callee = evaluate(expr.callee);

      List<Object> arguments = new ArrayList<>();
      for (Expr argument : expr.arguments) {
         arguments.add(evaluate(argument));
      }

      if (!(callee instanceof LoxCallable)) {
         throw new RuntimeError(expr.paren, "Can only call functions and classes.");
      }

      LoxCallable function = (LoxCallable) callee;
      if (arguments.size() != function.arity() && function.arity() != -1) {
         throw new RuntimeError(expr.paren, "Expected " + function.arity() + " arguments but got " + arguments.size() + ".");
      }

      return function.call(this, arguments);
   }

   @Override
   public Object visitGetExpr(Expr.Get expr) {
      Object obj = evaluate(expr.obj);
      if (obj instanceof LoxInstance) {
         return ((LoxInstance) obj).get(expr.name);
      }

      throw new RuntimeError(expr.name, "Only instances have properties.");
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitFunctionStmt(Stmt.Function stmt) {
      LoxFunction function = new LoxFunction(stmt, environment, false);
      environment.define(stmt.name.lexeme, function);
      return null;
   }

   /**
    *
    * @param stmt
    * @return
    */
   @Override
   public Object visitReturnStmt(Stmt.Return stmt) {
      Object value = null;
      if (stmt.value != null) {
         value = evaluate(stmt.value);
      }

      throw new Return(value);
   }
}
