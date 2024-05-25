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
    internal class RuntimeError : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly Token token;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        internal RuntimeError(Token token, String message): base(message)
        {
            this.token = token;    
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cause"></param>
        /// <param name="enableSuppression"></param>
        /// <param name="writableStackTrace"></param>
        internal RuntimeError(String message, Object cause, bool enableSuppression, bool writableStackTrace) : base(message)
        {
            token = null;
        }
    }
}
