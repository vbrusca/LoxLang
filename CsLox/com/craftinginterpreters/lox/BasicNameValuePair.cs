using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class BasicNameValuePair : NameValuePair
    {
        private readonly String name;
        private readonly String value;

        public BasicNameValuePair(String name, String value)
        {
            this.name = name;
            this.value = value;
        }

        public string getName()
        {
            return name;
        }

        public string getValue()
        {
            return value;
        }
    }
}
