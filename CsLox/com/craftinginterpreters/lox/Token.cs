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
    internal class Token
    {
        /// <summary>
        /// 
        /// </summary>
        internal TokenType type;

        /// <summary>
        /// 
        /// </summary>
        internal String lexeme;

        /// <summary>
        /// 
        /// </summary>
        internal Object literal;

        /// <summary>
        /// 
        /// </summary>
        internal int line;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="lexeme"></param>
        /// <param name="literal"></param>
        /// <param name="line"></param>
        internal Token(TokenType type, String lexeme, Object literal, int line)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.literal = literal;
            this.line = line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return type + " " + lexeme + " " + literal;
        }
    }
}
