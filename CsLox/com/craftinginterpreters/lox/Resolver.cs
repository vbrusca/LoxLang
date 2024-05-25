using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.craftinginterpreters.lox
{
    internal class Resolver : Expr.Visitor<Object>, Stmt.Visitor<Object>
    {
        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<String, bool>> scopes = new Stack<Dictionary<String, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        internal Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        internal void resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                resolve(statement);
            }
        }

        private void resolve(Stmt stmt)
        {
            stmt.accept(this);
        }

        private void resolve(Expr expr)
        {
            expr.accept(this);
        }

        private void resolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            beginScope();
            foreach (Token param in function.prms)
            {
                declare(param);
                define(param);
            }
            resolve(function.body);
            endScope();
            currentFunction = enclosingFunction;
        }

        private void beginScope()
        {
            //Lox.log(0, "Resolver.beginScope AAA " + scopes.Count);
            scopes.Push(new Dictionary<String, bool>());
            //Lox.log(0, "Resolver.beginScope BBB " + scopes.Count);
        }

        private void endScope()
        {
            //Lox.log(0, "Resolver.endScope AAA " + scopes.Count);
            scopes.Pop();
            //Lox.log(0, "Resolver.endScope BBB " + scopes.Count);
        }

        private void declare(Token name)
        {
            //Lox.log(0, "Resolver.declare " + name.lexeme + " scopes.size = " + scopes.Count);
            if (scopes.Count == 0) return;

            Dictionary<String, bool> scope = scopes.Peek();            
            if(scope.ContainsKey(name.lexeme))
            {
                Lox.error(name, "Already a variable with this name in this scope.");
            }
            
            scope[name.lexeme] = false;
            //Lox.log(0, "Resolver.define scope[" + name.lexeme + "] = false");
        }

        private void define(Token name)
        {
            //Lox.log(0, "Resolver.define " + name.lexeme + " scopes.size = " + scopes.Count); 
            if (scopes.Count == 0) return;

            Dictionary<String, bool> scope = scopes.Peek();
            scope[name.lexeme] = true;
            //Lox.log(0, "Resolver.define scope[" + name.lexeme + "] = true");
        }

        private void resolveLocal(Expr expr, Token name)
        {
            /*
            Lox.log(0, "Resolver.resolveLocal scopes.size = " + scopes.Count);
            Lox.log(0, "Resolver.resolveLocal expr = '" + expr + "', name = '" + name.literal + "'");
            for (int z = 0; z < scopes.Count; z++)
            {
                Lox.log(0, "Resolver.resolveLocal YYY index = " + z + ", size = " + scopes.ToArray()[z].Keys.Count);
                for (int j = 0; j < scopes.ToArray()[z].Keys.Count; j++)
                {
                    Lox.log(0, "Resolver.resolveLocal YYY key = " + scopes.ToArray()[z].Keys.ToArray()[j]);
                }
            }
            */

            //for (int i = scopes.Count - 1; i >= 0; i--)
            for (int i = 0; i < scopes.Count; i++)
            {
                //Lox.log(0, "Resolver.resolveLocal AAA index = " + i + ", size = " + scopes.ToArray()[i].Count + " looking for = '" + name.lexeme + "'");
                if (scopes.ToArray()[i].ContainsKey(name.lexeme))
                {
                    //Lox.log(0, "Resolver.resolveLocal BBB found = '" + name.lexeme + "', expr = '" + expr + "', distance = '" + i + "', " + (scopes.Count - 1 - i)); //
                    interpreter.resolve(expr, i);
                    return;
                }                
            }
        }

        public Object visitBlockStmt(Stmt.Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }

        public Object visitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            declare(stmt.name);
            define(stmt.name);

            if (stmt.superclass != null && stmt.name.lexeme == stmt.superclass.name.lexeme)
            {
                Lox.error(stmt.superclass.name, "A class can't inherit from itself.");
            }

            if (stmt.superclass != null)
            {
                currentClass = ClassType.SUBCLASS;
                resolve(stmt.superclass);
            }

            if (stmt.superclass != null)
            {
                beginScope();
                scopes.Peek()["super"] = true;
            }

            beginScope();
            scopes.Peek()["this"] = true;

            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init")
                {
                    declaration = FunctionType.INITIALIZER;
                }

                resolveFunction(method, declaration);
            }

            endScope();

            if (stmt.superclass != null) endScope();

            currentClass = enclosingClass;
            return null;
        }

        public Object visitExpressionStmt(Stmt.Expression stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public Object visitFunctionStmt(Stmt.Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public Object visitIfStmt(Stmt.If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null)
            {
                resolve(stmt.elseBranch);
            }
            return null;
        }

        public Object visitPrintStmt(Stmt.Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public Object visitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Lox.error(stmt.keyword, "Can't return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Lox.error(stmt.keyword, "Can't return a value from an initializer.");
                }

                resolve(stmt.value);
            }

            return null;
        }

        public Object visitVarStmt(Stmt.Var stmt)
        {
            declare(stmt.name);
            if (stmt.initializer != null)
            {
                resolve(stmt.initializer);
            }
            define(stmt.name);
            return null;
        }

        public Object visitWhileStmt(Stmt.While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }

        public Object visitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count != 0 && scopes.Peek().ContainsKey(expr.name.lexeme) && scopes.Peek()[expr.name.lexeme] == false)
            {
                Lox.error(expr.name, "Can't read local variable in its own initializer.");
            }

            resolveLocal(expr, expr.name);
            return null;
        }

        public Object visitAssignExpr(Expr.Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public Object visitBinaryExpr(Expr.Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public Object visitCallExpr(Expr.Call expr)
        {
            resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                resolve(argument);
            }

            return null;
        }

        public Object visitGetExpr(Expr.Get expr)
        {
            resolve(expr.obj);
            return null;
        }

        public Object visitGroupingExpr(Expr.Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public Object visitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public Object visitLogicalExpr(Expr.Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public Object visitSetExpr(Expr.Set expr)
        {
            resolve(expr.value);
            resolve(expr.obj);
            return null;
        }

        public Object visitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.error(expr.keyword, "Can't use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Lox.error(expr.keyword, "Can't use 'super' in a class with no superclass.");
            }

            resolveLocal(expr, expr.keyword);
            return null;
        }

        public Object visitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            resolveLocal(expr, expr.keyword);
            return null;
        }

        public Object visitUnaryExpr(Expr.Unary expr)
        {
            resolve(expr.right);
            return null;
        }
    }
}
