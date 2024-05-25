using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    public class ExternalLoxCallable : LoxCallable
    {
        internal Dictionary<Object, HandleLoxCallables> handleCalls;

        internal ExternalLoxCallable(Dictionary<Object, HandleLoxCallables> handleCalls)
        {
            this.handleCalls = handleCalls;
        }

        public int arity()
        {
            return -1;
        }

        object LoxCallable.call(Interpreter interpreter, List<object> arguments)
        {
            if (handleCalls != null && arguments.Count >= 1 && handleCalls.ContainsKey(arguments[0]))
            {
                HandleLoxCallables handleCall = handleCalls[arguments[0]];
                return handleCall.call(arguments.Count - 1, interpreter, arguments);
            }
            return null;
            //throw new NotImplementedException();
        }

        public override String ToString()
        {
            return "<native external fn>";
        }
    }
}
