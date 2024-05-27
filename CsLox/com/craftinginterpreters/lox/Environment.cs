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
    internal class Environment
    {
        /// <summary>
        /// 
        /// </summary>
        internal readonly Environment enclosing;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<String, Object> values = new Dictionary<String, Object>();

        /// <summary>
        /// 
        /// </summary>
        internal Environment()
        {
            enclosing = null;
        }

        public Dictionary<String, Object> getValues()
        {
            return new Dictionary<String, Object>(values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enclosing"></param>
        internal Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal Object get(Token name)
        {
            if (values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            if (enclosing != null) return enclosing.get(name);

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void assign(Token name, Object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void define(String name, Object value)
        {
            //Lox.log(0, "Environment.define: " + name + ", '" + value + "'");
            values[name] = value;
        }

        internal Environment ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment;
        }

        internal Object getAt(int distance, String name)
        {
            //Lox.log(0, "Environment.getAt: " + distance + ", '" + name + "'");
            Environment env = ancestor(distance);
            if (env != null && env.values != null && env.values.ContainsKey(name))
            {
                //Lox.log(0, "Environment.getAt: AAA: '" + env.values[name] + "'");
                return env.values[name];
            }
            else
            {
                //Lox.log(0, "Environment.getAt: BBB: '" + null + "'");
                return null;
            }
        }

        internal void assignAt(int distance, Token name, Object value)
        {
            //Lox.log(0, "Environment.assignAt: " + distance + ", '" + name.lexeme + "', '" + value + "'");
            ancestor(distance).values[name.lexeme]= value;
        }
    }
}
