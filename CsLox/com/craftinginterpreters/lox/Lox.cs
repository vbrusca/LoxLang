using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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
        /// <summary>
        /// 
        /// </summary>
        private static readonly Interpreter interpreter = new Interpreter();

        /// <summary>
        /// 
        /// </summary>
        internal static bool hadError = false;

        /// <summary>
        /// 
        /// </summary>
        internal static bool hadRuntimeError = false;

        /// <summary>
        /// 
        /// </summary>
        internal static String lastError = null;

        internal static String globals = null;
        internal static bool hasGlobals = false;

        internal static String lastFile = null;

        internal static String lastLine = null;

        /// <summary>
        /// The static main entry point to the program.
        /// Expects a valid Lox script file as an argument or no arguments if you want to start a REPL session.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            hasGlobals = false;
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i] == "-g")
                {
                    if(i + 1 < args.Length)
                    {
                        globals = args[i + 1];
                        hasGlobals = true;
                        break;
                    }
                }
            }

            if(hasGlobals)
            {
                System.Console.Out.WriteLine("Found global file to import: " + globals);
                runFile(globals);
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
            else
            {
                String str = "Usage: CsLox ([-f script] | [-s string] | [-p]) & [-g script]";
                System.Diagnostics.Debug.WriteLine(str);
                System.Console.Out.WriteLine(str);
                System.Environment.Exit(64);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
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

        /// <summary>
        /// 
        /// </summary>
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
                catch (Exception e)
                {
                    //do nothing
                }

                hadError = false;
                hadRuntimeError = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="message"></param>
        internal static void log(int line, String message)
        {
            String str = "[line " + line + "] Log: " + message;
            System.Diagnostics.Debug.WriteLine(str);
            System.Console.Out.WriteLine(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="message"></param>
        internal static void error(int line, String message)
        {
            report(line, "", message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="where"></param>
        /// <param name="message"></param>
        private static void report(int line, String where, String message)
        {
            lastError = "[line " + line + "] Error" + where + ": " + message;
            System.Diagnostics.Debug.WriteLine(lastError);
            System.Console.Error.WriteLine(lastError);
            hadError = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        internal static void runtimeError(RuntimeError error)
        {
            String msg = error.Message + "\n[line " + error.token.line + "]";
            System.Diagnostics.Debug.WriteLine(msg);
            System.Console.Error.WriteLine(msg);
            hadRuntimeError = true;
        }
    }
}
