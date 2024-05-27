using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static com.craftinginterpreters.lox.TokenType;

namespace com.craftinginterpreters.lox
{
    internal class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<Object>
    {
        internal readonly Environment globals = new Environment();
        internal readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        private Environment environment;
        public Dictionary<Object, HandleLoxCallables> handleCalls;
        public ExternalLoxCallable externalFunctions;
        public HandleLoxGlobals externalGlobals;
        public int currentLine = 0;

        public Interpreter()
        {
            this.handleCalls = new Dictionary<Object, HandleLoxCallables>();
            this.externalGlobals = null;
            initialize();
        }

        public Interpreter(Dictionary<Object, HandleLoxCallables> handleCalls)
        {
            this.handleCalls = handleCalls;
            this.externalGlobals = null;
            initialize();
        }

        public Interpreter(Dictionary<Object, HandleLoxCallables> handleCalls, HandleLoxGlobals externalGlobals)
        {
            this.handleCalls = handleCalls;
            this.externalGlobals = externalGlobals;
            initialize();
        }

        private void initialize()
        {
            environment = globals;
            globals.define("clock", new Clock());
            globals.define("Array", new Array());
            globals.define("List", new LinkedList());
            globals.define("Dict", new Dict());

            externalFunctions = new ExternalLoxCallable(handleCalls);
            globals.define("sys", externalFunctions);
            globals.define("GBL_NUM_MIN", Double.MaxValue * -1.0);       //-1.7976931348623157E+308
            globals.define("GBL_NUM_MAX", Double.MaxValue);              // 1.7976931348623157E+308

            if (externalGlobals != null)
            {
                externalGlobals.defineGlobals(globals);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statements"></param>
        internal void interpret(List<Stmt> statements)
        {
            currentLine = 0;
            try
            {
                foreach (Stmt statement in statements)
                {
                    execute(statement);
                    currentLine++;
                }
            }
            catch (RuntimeError error)
            {
                Lox.runtimeError(error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        private Object evaluate(Expr expr)
        {
            return expr.accept(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stmt"></param>
        private void execute(Stmt stmt)
        {
            stmt.accept(this);
        }

        internal void resolve(Expr expr, int depth)
        {
            //Lox.log(0, "Interpreter.resolve expr = '" + expr.GetType() + "', depth = '" + depth + "'");
            locals.Add(expr, depth);
            //locals[expr] = depth;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="environment"></param>
        internal void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public Object visitBinaryExpr(Expr.Binary expr)
        {
            //Lox.log(0, "Interpreter.visitBinaryExpr: left = '" + expr.left + "' right = '" + expr.right + "', oprtr = '" + expr.oprtr + "'");
            Object left = evaluate(expr.left);
            Object right = evaluate(expr.right);

            switch (expr.oprtr.type)
            {
                case BANG_EQUAL: return !isEqual(left, right);
                case EQUAL_EQUAL: return isEqual(left, right);
                case GREATER:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 > (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left > 0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 >= (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left >= 0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left >= (double)right;
                case LESS:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 < (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left < 0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 <= (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left <=  0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left <= (double)right;
                case MINUS:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 - (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left - 0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left - (double)right;
                case PLUS:
                    /*
                    if (left != null) Lox.log(0, "Left type: " + left.GetType());
                    if (right != null) Lox.log(0, "Right type: " + right.GetType());
                    Lox.log(0, "Arg Types: left = '" + left + "', right = '" + right + "'");
                    */

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 + (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left + 0.0;
                    }

                    if ((left is Double && right is Double) || (left is double && right is double)) {
                        return (double)left + (double)right;
                    }

                    if (right is String && left == null)
                    {
                        return "" + (String)right;
                    }

                    if (left is String && right == null)
                    {
                        return (String)left + "";
                    }

                    if (left is String && right is String) {
                        return (String)left + (String)right;
                    }

                    throw new RuntimeError(expr.oprtr, "Operands must be two numbers or two strings.");
                case SLASH:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 / (double)right;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left / (double)right;
                case STAR:

                    if ((right is Double || right is double) && left == null)
                    {
                        return 0.0 * (double)right;
                    }

                    if ((left is Double || left is double) && right == null)
                    {
                        return (double)left * 0.0;
                    }

                    checkNumberOperands(expr.oprtr, left, right);
                    return (double)left * (double)right;
            }

            // Unreachable.
            Lox.log(currentLine, "Warning! About to return null from the Interpeter's visitBinaryExpr method. Expr: " + expr + " Oprtr Type:" + expr.oprtr.type);
            return null;
        }

        public Object visitGroupingExpr(Expr.Grouping expr)
        {
            return evaluate(expr.expression);
        }

        public Object visitLiteralExpr(Expr.Literal expr)
        {
            //if(expr.value == null)
            //{
            //    Lox.log(0, "Interpreter.visitLiteralExpr expr.value is null");
            //}
            //Lox.log(0, "Interpreter.visitLiteralExpr expr.value = '" + expr.value + "'");
            return expr.value;
        }

        public Object visitUnaryExpr(Expr.Unary expr)
        {
            Object right = evaluate(expr.right);

            switch (expr.oprtr.type)
            {
                case BANG:
                    return !isTruthy(right);
                case MINUS:
                    checkNumberOperand(expr.oprtr, right);
                    return -(double)right;
            }

            // Unreachable.
            Lox.log(currentLine, "Warning! About to return null from the Interpeter's visitUnaryExpr method.");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oprtr"></param>
        /// <param name="operand"></param>
        /// <exception cref="RuntimeError"></exception>
        private void checkNumberOperand(Token oprtr, Object operand)
        {
            if (operand is Double || operand is double) return;
            throw new RuntimeError(oprtr, "Operand must be a number.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oprtr"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <exception cref="RuntimeError"></exception>
        private void checkNumberOperands(Token oprtr, Object left, Object right)
        {
            if ((left is Double && right is Double) || (left is double && right is double)) return;

            throw new RuntimeError(oprtr, "Operands must be numbers.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool isTruthy(Object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool isEqual(Object a, Object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private String stringify(Object obj)
        {
            if (obj == null) return "null"; //"nil";

            if (obj is Double || obj is double) {
                String text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        public Object visitExpressionStmt(Stmt.Expression stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public Object visitPrintStmt(Stmt.Print stmt)
        {
            Object value = evaluate(stmt.expression);
            String str = stringify(value);
            if (value is bool || value is Boolean || (value != null && value.GetType() != null && value.GetType().Name == "Boolean"))
            {
                str = str.ToLower();
            }
            System.Diagnostics.Debug.WriteLine(str);
            System.Console.Out.WriteLine(str);
            return null;
        }

        public Object visitVariableExpr(Expr.Variable expr)
        {
            //Lox.log(0, "visitVariableExpr: '" + expr.name.lexeme + "'");
            return lookUpVariable(expr.name, expr); //environment.get(expr.name);
        }

        private Object lookUpVariable(Token name, Expr expr)
        {
            //Lox.log(0, "LookUpVariable: '" + name.lexeme + "': " + expr);
            int distance = int.MinValue;
            if (locals.ContainsKey(expr))
            {
                distance = locals[expr];
            }

            if (distance != int.MinValue)
            {
                //Lox.log(0, "LookUpVariable: 'AAA'");
                return environment.getAt(distance, name.lexeme);
            }
            else
            {
                //Lox.log(0, "LookUpVariable: 'BBB'");
                return globals.get(name);
            }
        }

        public Object visitVarStmt(Stmt.Var stmt)
        {
            Object value = null;
            if (stmt.initializer != null)
            {
                value = evaluate(stmt.initializer);
            }

            //Lox.log(0, "Interpreter.visitVarStmt: value = '" + value + "', init: '" + stmt.name + "', " + stmt.initializer);
            environment.define(stmt.name.lexeme, value);
            return null;
        }

        public Object visitAssignExpr(Expr.Assign expr)
        {
            Object value = evaluate(expr.value);

            int distance = int.MinValue;
            if (locals.ContainsKey(expr))
            {
                distance = locals[expr];
            }
                
            if (distance != int.MinValue)
            {
                environment.assignAt(distance, expr.name, value);
            }
            else
            {
                globals.assign(expr.name, value);
            }

            return value;
        }

        public Object visitBlockStmt(Stmt.Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object visitClassStmt(Stmt.Class stmt)
        {
            Object superclass = null;
            if (stmt.superclass != null)
            {
                superclass = evaluate(stmt.superclass);
                if (!(superclass is LoxClass))
                {
                    throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
                }
            }

            environment.define(stmt.name.lexeme, null);

            if (stmt.superclass != null)
            {
                environment = new Environment(environment);
                environment.define("super", superclass);
            }

            Dictionary<String, LoxFunction> methods = new Dictionary<String, LoxFunction>();
            foreach (Stmt.Function method in stmt.methods)
            {
                LoxFunction function = new LoxFunction(method, environment, (method.name.lexeme == "init"));
                methods[method.name.lexeme] = function;
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, (LoxClass)superclass, methods);

            if (superclass != null)
            {
                environment = environment.enclosing;
            }

            environment.assign(stmt.name, klass);
            return null;
        }

        public Object visitIfStmt(Stmt.If stmt)
        {
            if (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }
            return null;
        }

        public Object visitLogicalExpr(Expr.Logical expr)
        {
            Object left = evaluate(expr.left);

            if (expr.oprtr.type == TokenType.OR) {
                if (isTruthy(left)) return left;
            } 
            else
            {
                if (!isTruthy(left)) return left;
            }

            return evaluate(expr.right);
        }

        public Object visitSetExpr(Expr.Set expr)
        {
            Object obj = evaluate(expr.obj);

            if (!(obj is LoxInstance))
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            Object value = evaluate(expr.value);
            ((LoxInstance)obj).set(expr.name, value);
            return value;
        }

        public Object visitSuperExpr(Expr.Super expr)
        {
            int distance = locals[expr];
            LoxClass superclass = (LoxClass)environment.getAt(distance, "super");

            LoxInstance obj = (LoxInstance)environment.getAt(distance - 1, "this");

            LoxFunction method = superclass.findMethod(expr.method.lexeme);

            if (method == null)
            {
                throw new RuntimeError(expr.method, "Undefined property '" + expr.method.lexeme + "'.");
            }

            return method.bind(obj);
        }

        public Object visitThisExpr(Expr.This expr)
        {
            return lookUpVariable(expr.keyword, expr);
        }

        public Object visitWhileStmt(Stmt.While stmt)
        {
            while (isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }
            return null;
        }

        public Object visitCallExpr(Expr.Call expr)
        {
            Object callee = evaluate(expr.callee);

            List<Object> arguments = new List<Object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if (!(callee is LoxCallable)) {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;
            if (arguments.Count != function.arity() && function.arity() != -1)
            {
                throw new RuntimeError(expr.paren, "Expected " + function.arity() + " arguments but got " + arguments.Count + ".");
            }

            return function.call(this, arguments);
        }

        public Object visitGetExpr(Expr.Get expr)
        {
            Object obj = evaluate(expr.obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public Object visitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.define(stmt.name.lexeme, function);
            return null;
        }

        public Object visitReturnStmt(Stmt.Return stmt)
        {
            Object value = null;
            if (stmt.value != null)
            {
                value = evaluate(stmt.value);
            }

            throw new Return(value);
        }
    }
}
