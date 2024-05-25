package com.craftinginterpreters.lox;

import java.util.HashMap;
import java.util.Map;

/**
 *
 * @author brusc
 */
class ExternalLoxGlobal {
        public Map<String, Object> externalGlobals;

        public ExternalLoxGlobal()
        {
            externalGlobals = new HashMap<>();
        }

        public ExternalLoxGlobal(HashMap<String, Object> externalGlobals)
        {
            this.externalGlobals = externalGlobals;
        }

        public void defineGlobals(Environment loxGlobals)
        {
            if (externalGlobals != null)
            {
                for (String key : externalGlobals.keySet())
                {
                    loxGlobals.define(key, externalGlobals.get(key));
                }
            }
        }
}
