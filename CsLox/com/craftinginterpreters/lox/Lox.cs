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
        private static readonly Interpreter interpreter = new Interpreter();
        internal static bool hadError = false;
        internal static bool hadRuntimeError = false;
        internal static String lastError = null;

        internal static String globalsFile = null;
        internal static String globalsScript = null;
        internal static bool hasGlobalsFile = false;
        internal static bool hasGlobalsScript = false;
        internal static String lastFile = null;
        internal static String lastLine = null;

        /// <summary>
        /// The static main entry point to the program.
        /// Expects a valid Lox script file as an argument or no arguments if you want to start a REPL session.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
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
                String str = "Usage: CsLox  ([-f script file] | [-s script string] | [-p REPL]) & [-gf script file] [-gs script string]";
                System.Diagnostics.Debug.WriteLine(str);
                System.Console.Out.WriteLine(str);
                System.Environment.Exit(64);
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
