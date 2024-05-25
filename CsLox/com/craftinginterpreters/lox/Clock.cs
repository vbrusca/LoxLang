using com.craftinginterpreters.lox;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    class Clock : LoxCallable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int arity() { 
            return 0; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            if (Settings.TIMING == Settings.TimingTypes.TICKS_FROM_APP_START)
            {
                return (double)System.Environment.TickCount;
            }
            else if (Settings.TIMING == Settings.TimingTypes.SECONDS_FROM_EPOCH)
            {
                return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else
            {
                return (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<native fn>";
        }
    }
}