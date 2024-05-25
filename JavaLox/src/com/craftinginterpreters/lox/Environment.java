package com.craftinginterpreters.lox;

import java.util.HashMap;
import java.util.Map;

/**
 *
 * @author brusc
 */
class Environment {

   /**
    *
    */
   final Environment enclosing;

   /**
    *
    */
   private final Map<String, Object> values = new HashMap<>();

   /**
    *
    */
   Environment() {
      enclosing = null;
   }

   /**
    *
    * @param enclosing
    */
   Environment(Environment enclosing) {
      this.enclosing = enclosing;
   }

   /**
    *
    * @param name
    * @return
    */
   Object get(Token name) {
      if (values.containsKey(name.lexeme)) {
         return values.get(name.lexeme);
      }

      if (enclosing != null) {
         return enclosing.get(name);
      }

      throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
   }

   /**
    *
    * @param name
    * @param value
    */
   void assign(Token name, Object value) {
      if (values.containsKey(name.lexeme)) {
         values.put(name.lexeme, value);
         return;
      }

      if (enclosing != null) {
         enclosing.assign(name, value);
         return;
      }

      throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
   }

   /**
    *
    * @param name
    * @param value
    */
   void define(String name, Object value) {
      //Lox.log(0, "Environment.define: " + name + ", '" + value + "'");      
      values.put(name, value);
   }

   Environment ancestor(int distance) {
      Environment environment = this;
      for (int i = 0; i < distance; i++) {
         environment = environment.enclosing;
      }

      return environment;
   }

   Object getAt(int distance, String name) {
      //Lox.log(0, "Environment.getAt: " + distance + ", '" + name + "'");
      Environment env = ancestor(distance);
      if (env != null && env.values != null && env.values.containsKey(name)) {
         //Lox.log(0, "Environment.getAt: AAA: '" + env.values.get(name) + "'");
         return env.values.get(name);
      } else {
         //Lox.log(0, "Environment.getAt: BBB: '" + null + "'");         
         return null;
      }
   }

   void assignAt(int distance, Token name, Object value) {
      //Lox.log(0, "Environment.assignAt: " + distance + ", '" + name.lexeme + "', '" + value + "'");
      ancestor(distance).values.put(name.lexeme, value);
   }
}
