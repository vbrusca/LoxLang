package com.craftinginterpreters.lox;

import java.util.List;
import java.util.Map;

/**
 *
 * @author brusc
 */
public class ExternalLoxCallable implements LoxCallable {

   Map<Object, HandleLoxCallables> handleCalls;

   ExternalLoxCallable(Map<Object, HandleLoxCallables> handleCalls) {
      this.handleCalls = handleCalls;
   }

   @Override
   public int arity() {
      return -1;
   }

   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {
      if (handleCalls != null && arguments.size() >= 1 && handleCalls.containsKey(arguments.get(0))) {
         HandleLoxCallables handleCall = handleCalls.get(arguments.get(0));
         return handleCall.call(arguments.size() - 1, interpreter, arguments);
      }
      return null;
      //throw new UnsupportedOperationException("Not supported yet."); // Generated from nbfs://nbhost/SystemFileSystem/Templates/Classes/Code/GeneratedMethodBody
   }

   @Override
   public String toString() {
      return "<native external fn>";
   }
}
