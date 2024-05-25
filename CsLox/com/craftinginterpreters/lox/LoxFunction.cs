using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    internal class LoxFunction : LoxCallable
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Stmt.Function declaration;

        /// <summary>
        /// 
        /// </summary>
        private readonly Environment closure;

        private readonly bool isInitializer;

        /*
        internal LoxFunction(Stmt.Function declaration)
        {
            this.closure = null;
            this.declaration = declaration;
        }
        */

        internal LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
        {
            this.isInitializer = isInitializer;
            this.closure = closure;
            this.declaration = declaration;
        }

        internal LoxFunction bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            environment.define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Object call(Interpreter interpreter, List<Object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.prms.Count; i++)
            {
                environment.define(declaration.prms[i].lexeme, arguments[i]);
            }

            try 
            {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (Return returnValue)
            {
                if (isInitializer) return closure.getAt(0, "this");

                return returnValue.value;
            }

            if (isInitializer) return closure.getAt(0, "this");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int arity()
        {
            return declaration.prms.Count;
        }
    }
}
