package com.craftinginterpreters.lox;

/**
 *
 * @author brusc
 */
class Settings {
   /**
    * 
    */
   enum TimingTypes {
      TICKS_FROM_APP_START,
      MILLISECONDS_FROM_EPOCH,
      SECONDS_FROM_EPOCH
   }

   /**
    * 
    */
   static TimingTypes TIMING = TimingTypes.MILLISECONDS_FROM_EPOCH;
}
