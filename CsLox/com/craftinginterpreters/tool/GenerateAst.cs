using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.tool
{
    /// <summary>
    /// 
    /// </summary>
    public class GenerateAst
    {
        /// <summary>
        /// 
        /// </summary>
        static bool RUN_EXPR = true;

        /// <summary>
        /// 
        /// </summary>
        static bool RUN_STMT = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            //if (args.Length != 1)
            //{
                args = new String[1];
                args[0] = "C:\\FILES\\DOCUMENTS\\GitHub\\LoxLang\\CsLox\\com\\craftinginterpreters\\lox";
                System.Console.Out.WriteLine("Usage: generate_ast [output directory]");
                //System.Environment.Exit(64);
            //}
            String outputDir = args[0];

            if (RUN_EXPR)
            {
                String[] typesA = new String[] {
                    "Assign   : Token name, Expr value",
                    "Binary   : Expr left, Token oprtr, Expr right",
                    "Call     : Expr callee, Token paren, List<Expr> arguments",
                    "Get      : Expr obj, Token name",
                    "Grouping : Expr expression",
                    "Literal  : Object value",
                    "Logical  : Expr left, Token oprtr, Expr right",
                    "Set      : Expr obj, Token name, Expr value",
                    "Super    : Token keyword, Token method",
                    "This     : Token keyword",
                    "Unary    : Token oprtr, Expr right",
                    "Variable : Token name"
                };

                defineAst(outputDir,
                    "Expr",
                    typesA.ToList<String>()
                );
            }

            if (RUN_STMT)
            {
                String[] typesB = new String[] {
                    "Block      : List<Stmt> statements",
                    "Class      : Token name, Expr.Variable superclass, List<Stmt.Function> methods",
                    "Expression : Expr expression",
                    "Function   : Token name, List<Token> prms, List<Stmt> body",
                    "If         : Expr condition, Stmt thenBranch, Stmt elseBranch",
                    "Print      : Expr expression",
                    "Return     : Token keyword, Expr value",
                    "Var        : Token name, Expr initializer",
                    "While      : Expr condition, Stmt body"
                };
                defineAst(outputDir, "Stmt", typesB.ToList<String>());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outputDir"></param>
        /// <param name="baseName"></param>
        /// <param name="types"></param>
        private static void defineAst(String outputDir, String baseName, List<String> types)
        {
            String path = outputDir + "/" + baseName + ".cs";
            StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Create), Encoding.UTF8);

            //imports
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine();
            writer.WriteLine("namespace com.craftinginterpreters.lox {");
            writer.WriteLine();
            writer.WriteLine("    internal abstract class " + baseName + " {");

            defineVisitor(writer, baseName, types);
            writer.WriteLine();
            writer.WriteLine();

            // The AST classes.
            foreach (String type in types)
            {
                String className = type.Split(':')[0].Trim();
                String fields = type.Split(':')[1].Trim();
                defineType(writer, baseName, className, fields);
            }

            // The base accept() method.
            writer.WriteLine();
            writer.WriteLine("        public abstract R accept<R>(Visitor<R> visitor);");

            writer.WriteLine("    }");
            writer.WriteLine("}");
            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="baseName"></param>
        /// <param name="types"></param>
        private static void defineVisitor(StreamWriter writer, String baseName, List<String> types)
        {
            writer.WriteLine("        public interface Visitor<R> {");

            foreach(String type in types)
            {
                String typeName = type.Split(':')[0].Trim();
                writer.WriteLine("            R visit" + typeName + baseName + "(" + typeName + " " + baseName.ToLower() + ");");
            }

            writer.WriteLine("        }");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="baseName"></param>
        /// <param name="className"></param>
        /// <param name="fieldList"></param>
        private static void defineType(StreamWriter writer, String baseName, String className, String fieldList)
        {
            writer.WriteLine("        public class " + className + " : " + baseName + " {");

            // Constructor.
            writer.WriteLine("            public " + className + "(" + fieldList + ") {");

            // Store parameters in fields.
            String[] fields = fieldList.Split(new String[] {", "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (String field in fields)
            {
                String name = field.Split(' ')[1];
                writer.WriteLine("                this." + name + " = " + name + ";");
            }

            writer.WriteLine("            }");

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine("            public override R accept<R>(Visitor<R> visitor) {");
            writer.WriteLine("                return visitor.visit" +
                className + baseName + "(this);");
            writer.WriteLine("            }");

            // Fields.
            writer.WriteLine();
            foreach (String field in fields)
            {
                writer.WriteLine("            public readonly " + field + ";");
            }

            writer.WriteLine("        }");
            writer.WriteLine();
            writer.WriteLine();
        }
    }
}
