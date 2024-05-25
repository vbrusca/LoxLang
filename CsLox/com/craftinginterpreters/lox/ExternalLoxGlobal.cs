using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class ExternalLoxGlobal : HandleLoxGlobals
    {
        public Dictionary<String, Object> externalGlobals;

        public ExternalLoxGlobal()
        {
            externalGlobals = new Dictionary<String, Object>();
        }

        public ExternalLoxGlobal(Dictionary<String, Object> externalGlobals)
        {
            this.externalGlobals = externalGlobals;
        }

        public void defineGlobals(Environment loxGlobals)
        {
            if (externalGlobals != null)
            {
                foreach (String key in externalGlobals.Keys)
                {
                    loxGlobals.define(key, externalGlobals[key]);
                }
            }
        }
    }
}
