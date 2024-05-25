package com.craftinginterpreters.lox;

import java.util.List;

/**
 *
 * @author brusc
 */
class LoxFunction implements LoxCallable {

   /**
    *
    */
   private final Stmt.Function declaration;

   /**
    *
    */
   private final Environment closure;

   private final boolean isInitializer;

   /*
   LoxFunction(Stmt.Function declaration) {
      this.closure = null;
      this.declaration = declaration;
   }
    */
   LoxFunction(Stmt.Function declaration, Environment closure, boolean isInitializer) {
      this.isInitializer = isInitializer;
      this.closure = closure;
      this.declaration = declaration;
   }

   LoxFunction bind(LoxInstance instance) {
      Environment environment = new Environment(closure);
      environment.define("this", instance);
      return new LoxFunction(declaration, environment, isInitializer);
   }

   /**
    *
    * @param interpreter
    * @param arguments
    * @return
    */
   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      Environment environment = new Environment(closure);
      for (int i = 0; i < declaration.prms.size(); i++) {
         environment.define(declaration.prms.get(i).lexeme, arguments.get(i));
      }

      try {
         interpreter.executeBlock(declaration.body, environment);
      } catch (Return returnValue) {
         if (isInitializer) {
            return closure.getAt(0, "this");
         }
         
         return returnValue.value;
      }

      if (isInitializer) {
         return closure.getAt(0, "this");
      }
      return null;
   }

   /**
    *
    * @return
    */
   @Override
   public int arity() {
      return declaration.prms.size();
   }
}
