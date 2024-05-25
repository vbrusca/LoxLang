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
    internal class Return : RuntimeError
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly Object value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        internal Return(Object value) : base(null, null, false, false)
        {
            this.value = value;
        }
    }
}
