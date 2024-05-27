using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using System.Net.Http;
using System.Web;
using System.Security.Policy;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// An implementation of the "Lox" embedded scripting language.
    /// This work was created from tutorials by the original author, Robert Nystrom.
    /// Source URL: https://craftinginterpreters.com/ 
    /// 
    /// @author Victor G.Brusca, Carlo Bruscani, Copyright Middlemind Games 05/03/2024
    /// </summary>
    public class Lox
    {
        public enum UrlConnType
        {
            GET_BLANK,
            POST,
            GET_QUERY_PARAMS
        }

        private static readonly Interpreter interpreter = new Interpreter();
        internal static bool hadError = false;
        internal static bool hadRuntimeError = false;
        internal static String lastError = null;

        internal static String globalsFile = null;
        internal static String globalsScript = null;
        internal static String globalsUrl = null;
        internal static bool hasGlobalsFile = false;
        internal static bool hasGlobalsScript = false;
        internal static bool hasGlobalsUrl = false;
        internal static String lastFile = null;
        internal static String lastLine = null;
        internal static String lastUrl = null;

        internal static bool hasReturnUrl = false;
        internal static String returnUrl = null;
        internal static String returnUrlGlobalVarName = null;

        /// <summary>
        /// The static main entry point to the program.
        /// Expects a valid Lox script file as an argument or no arguments if you want to start a REPL session.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            /*
            String url = "";
            List<NameValuePair> data = new List<NameValuePair>();
            data.Add(new BasicNameValuePair("answer", "hello"));

            url = "http://localhost:5223/getGlobal";
            Lox.log(0, "Calling getBlank URL = '" + url + "', Response = \n" + getBlank(url));

            url = "https://localhost:7109/getGlobal";
            Lox.log(0, "Calling getBlank URL = '" + url + "', Response = \n" +  getBlank(url));

            url = "http://localhost:5223/getGlobal";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "https://localhost:7109/getGlobal";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "http://localhost:5223/getGlobal";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            url = "https://localhost:7109/getGlobal";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            url = "http://localhost:5223/getScript";
            Lox.log(0, "Calling getBlank URL = '" + url + "', Response = \n" + getBlank(url));

            url = "https://localhost:7109/getScript";
            Lox.log(0, "Calling getBlank URL = '" + url + "', Response = \n" + getBlank(url));

            url = "http://localhost:5223/getScript";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "https://localhost:7109/getScript";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "http://localhost:5223/getScript";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            url = "https://localhost:7109/getScript";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            url = "http://localhost:5223/setAnswer";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "https://localhost:7109/setAnswer";
            Lox.log(0, "Calling getQueryParams URL = '" + url + "', Response = \n" + getQueryParams(url, data));

            url = "http://localhost:5223/setAnswer";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            url = "https://localhost:7109/setAnswer";
            Lox.log(0, "Calling post URL = '" + url + "', Response = \n" + post(url, data));

            return;
            */

            hasGlobalsFile = false;
            hasGlobalsScript = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i].Equals("-gf"))
                {
                    if (i + 1 < args.Length)
                    {
                        globalsFile = args[i + 1];
                        hasGlobalsFile = true;
                    }
                }
                else if (args[i] != null && args[i].Equals("-gs"))
                {
                    if (i + 1 < args.Length)
                    {
                        globalsScript = args[i + 1];
                        hasGlobalsScript = true;
                    }
                }
                else if (args[i] != null && args[i].Equals("-gu"))
                {
                    if (i + 1 < args.Length)
                    {
                        globalsUrl = args[i + 1];
                        hasGlobalsUrl = true;
                    }
                }
                else if (args[i] != null && args[i].Equals("-ru"))
                {
                    if (i + 1 < args.Length)
                    {
                        returnUrl = args[i + 1];
                        hasReturnUrl = true;
                    }
                }
                else if (args[i] != null && args[i].Equals("-gv"))
                {
                    if (i + 1 < args.Length)
                    {
                        returnUrlGlobalVarName = args[i + 1];
                    }
                }
            }

            if (hasGlobalsFile)
            {
                Lox.log(0, "Found global file to import: " + globalsFile);
                runFile(globalsFile);
            }

            if (hasGlobalsScript)
            {
                Lox.log(0, "Found global script to import: " + globalsScript);
                run(globalsScript);
            }

            if(hasGlobalsUrl)
            {
                Lox.log(0, "Found global script to import: " + globalsUrl);
                runUrl(globalsUrl, UrlConnType.GET_BLANK, null);
            }

            if (args.Length >= 1 && args[0] != null && args[0].ToLower().Equals("-p"))
            {
                runPrompt();
            }
            else if (args.Length >= 2 && args[0] != null && args[0].ToLower().Equals("-f") && args[1] != null)
            {
                runFile(args[1]);
            }
            else if (args.Length >= 2 && args[0] != null && args[0].ToLower().Equals("-s") && args[1] != null)
            {
                run(args[1]);
            }
            else if (args.Length >= 2 && args[0] != null && args[0].ToLower().Equals("-u") && args[1] != null)
            {
                runUrl(args[1], UrlConnType.GET_BLANK, null);
            }
            else
            {
                /*
                    -f "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\test.lox" 
                    -gs "var GBL_BASE_NAT_LOG = 2.71828;" 
                    -gf "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\globals.lox"
                */
                // -gs "var GBL_BASE_NAT_LOG = 2.71828;" -gf "C:\FILES\OIT_LAPTOP_BACKUP\DOCUMENTS\GitHub\LoxLang\globals.lox" -gu https://localhost:7109/getGlobal -u https://localhost:7109/getScript -ru https://localhost:7109/setAnswer -gv urlGlobal
                //-gu https://localhost:7109/getGlobal -u https://localhost:7109/getScript -ru https://localhost:7109/setAnswer -gv urlGlobal
                String str = "Usage: CsLox  ([-f script file] | [-s script string] | [-u script url] | [-p REPL]) & [-gf script file] [-gs script string] [-gu script url] [-ru url] [-gv global variable name to respond with]";
                System.Diagnostics.Debug.WriteLine(str);
                System.Console.Out.WriteLine(str);
                System.Environment.Exit(64);
            }

            if(hasReturnUrl)
            {
                Dictionary<String, Object> values = interpreter.globals.getValues();
                if (values.ContainsKey(returnUrlGlobalVarName))
                {
                    String value = (values[returnUrlGlobalVarName] + "");
                    List<NameValuePair> data = new List<NameValuePair>();
                    data.Add(new BasicNameValuePair("answer", value));
                    data.Add(new BasicNameValuePair("variableName", returnUrlGlobalVarName));
                    Lox.log(0, "Return URL Response: '" + getQueryParams(returnUrl, data) + "'");
                }
            }
        }

        private static String getBlank(String url)
        {
            Task<String> ret = getBlankWrkrAsync(url);
            return ret.Result;
        }

        private static async Task<String> getBlankWrkrAsync(String url)
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync(url);
        }

        private static String getQueryParams(String url, List<NameValuePair> data)
        {
            Task<String> ret = getQueryParamsWrkrAsync(url, data);
            return ret.Result;
        }

        private static async Task<String> getQueryParamsWrkrAsync(String url, List<NameValuePair> data)
        {
            HttpClient client = new HttpClient();
            String newUrl = url;
            if (data != null && data.Count > 0)
            {
                int len = data.Count;
                int cnt = 0;
                newUrl += "?";
                foreach (BasicNameValuePair nvp in data)
                {
                    newUrl += HttpUtility.UrlEncode(nvp.getName()) + "=" + HttpUtility.UrlEncode(nvp.getValue());
                    if(cnt < len - 1)
                    {
                        newUrl += "&";
                    }
                    cnt++;
                }
            }
            return await client.GetStringAsync(newUrl);
        }

        private static String post(String url, List<NameValuePair> data)
        {
            Task<String> ret = postWrkrAsync(url, data);
            return ret.Result;
        }

        private static async Task<String> postWrkrAsync(String url, List<NameValuePair> data)
        {
            HttpClient client = new HttpClient();
            Dictionary<String, String> values = new Dictionary<String, String>();
            if(data != null && data.Count > 0)
            {
                foreach (BasicNameValuePair nvp in data)
                {
                    values.Add(nvp.getName(), nvp.getValue());
                }
            }
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await client.PostAsync(url, content);
            Lox.log(0, "ResponseCode: " + response.IsSuccessStatusCode);
            return await response.Content.ReadAsStringAsync();
        }

        private static void runUrl(String url, UrlConnType connType, List<NameValuePair> data)
        {
            String res = "";
            lastUrl = url;
            if (connType == UrlConnType.GET_BLANK)
            {
                res = getBlank(url);
            }
            else if (connType == UrlConnType.GET_QUERY_PARAMS)
            {
                res = getQueryParams(url, data);
            }
            Lox.log(0, "Run URL Response: '" + res + "'");

            JsonScript value = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonScript>(res);
            if(value != null && value.script != "")
            {
                run(value.script);
            }
        }

        private static void runFile(String path)
        {
            lastFile = path;
            byte[] bytes = System.IO.File.ReadAllBytes(System.IO.Path.GetFullPath(path));
            //run(StringFromBytesOptimzed(bytes));              //internal ASCII alternative
            //run(System.Text.Encoding.UTF8.GetString(bytes));  //UTF8 charset alternative
            run(System.Text.Encoding.Default.GetString(bytes)); //default charset

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        private static void runPrompt()
        {
            //Control-D to exit
            for(; ;)
            {
                Console.Out.Write("> ");
                String line = Console.ReadLine();
                if (line == null || line.ToLower().Equals("exit") || line.ToLower().Equals("bye")) break;

                try
                {
                    run(line);
                } 
                catch (Exception e) {}

                hadError = false;
                hadRuntimeError = false;
            }
        }

        private static void run(String source)
        {
            lastLine = source;
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.scanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.resolve(statements);

            // Stop if there was a resolution error.
            if (hadError) return;

            interpreter.interpret(statements);
        }

        internal static void log(int line, String message)
        {
            String str = "[line " + line + "] Log: " + message;
            System.Diagnostics.Debug.WriteLine(str);
            System.Console.Out.WriteLine(str);
        }

        internal static void error(int line, String message)
        {
            report(line, "", message);
        }

        private static void report(int line, String where, String message)
        {
            lastError = "[line " + line + "] Error" + where + ": " + message;
            System.Diagnostics.Debug.WriteLine(lastError);
            System.Console.Error.WriteLine(lastError);
            hadError = true;
        }

        internal static void error(Token token, String message)
        {
            if (token.type == TokenType.EOF)
            {
                report(token.line, " at end", message);
            }
            else
            {
                report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        /// <summary>
        /// A helper function that converts a byte array to a string using char casting and a string builder.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string StringFromBytesOptimzed(byte[] b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append((char)b[i]);
            }
            return sb.ToString();
        }

        internal static void runtimeError(RuntimeError error)
        {
            String msg = error.Message + "\n[line " + error.token.line + "]";
            System.Diagnostics.Debug.WriteLine(msg);
            System.Console.Error.WriteLine(msg);
            hadRuntimeError = true;
        }
    }
}
