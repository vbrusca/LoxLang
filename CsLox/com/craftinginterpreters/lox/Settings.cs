using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        internal enum TimingTypes
        {
            TICKS_FROM_APP_START,
            MILLISECONDS_FROM_EPOCH,
            SECONDS_FROM_EPOCH
        }

        /// <summary>
        /// 
        /// </summary>
        internal static TimingTypes TIMING = TimingTypes.MILLISECONDS_FROM_EPOCH;
    }
}
