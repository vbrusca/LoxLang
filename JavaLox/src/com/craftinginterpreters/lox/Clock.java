package com.craftinginterpreters.lox;

import java.util.List;

/**
 *
 * @author brusc
 */
public class Clock implements LoxCallable {

   /**
    *
    * @return
    */
   @Override
   public int arity() {
      return 0;
   }

   /**
    *
    * @param interpreter
    * @param arguments
    * @return
    */
   @Override
   public Object call(Interpreter interpreter, List<Object> arguments) {      
      if (Settings.TIMING == Settings.TimingTypes.TICKS_FROM_APP_START) {
         return (double) System.nanoTime();
      } else if (Settings.TIMING == Settings.TimingTypes.SECONDS_FROM_EPOCH) {
         return (double) System.currentTimeMillis();
      } else {
         return (double) System.currentTimeMillis() / 1000.0;
      }
   }

   /**
    *
    * @return
    */
   @Override
   public String toString() {
      return "<native fn>";
   }
}
