using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static com.craftinginterpreters.lox.Expr;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    internal class AstPrinter : Expr.Visitor<String>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static void Main(String[] args)
        {
            Expr expression = new Expr.Binary(
                new Expr.Unary(
                    new Token(TokenType.MINUS, "-", null, 1),
                    new Expr.Literal(123)),
                new Token(TokenType.STAR, "*", null, 1),
                new Expr.Grouping(
                    new Expr.Literal(45.67)));

            String str = new AstPrinter().print(expression);
            System.Console.Out.WriteLine(str);
            System.Diagnostics.Debug.WriteLine(str);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        internal String print(Expr expr)
        {
            return expr.accept(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>        
        public String visitBinaryExpr(Expr.Binary expr)
        {
            return parenthesize(expr.oprtr.lexeme, new Expr[] { expr.left, expr.right });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public String visitGroupingExpr(Expr.Grouping expr)
        {
            return parenthesize("group", expr.expression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public String visitLiteralExpr(Expr.Literal expr)
        {
            if (expr.value == null) return "nil";
            return expr.value.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public String visitUnaryExpr(Expr.Unary expr)
        {
            return parenthesize(expr.oprtr.lexeme, expr.right);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="expr"></param>
        /// <returns></returns>
        private String parenthesize(String name, Expr expr)
        {
            return parenthesize(name, new Expr[] { expr });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private String parenthesize(String name, Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        string Visitor<string>.visitVariableExpr(Variable expr)
        {
            throw new NotImplementedException();
        }
    }
}
