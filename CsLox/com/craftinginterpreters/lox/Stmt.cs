using System;
using System.Collections.Generic;
using System.Text;

namespace com.craftinginterpreters.lox {

    internal abstract class Stmt {
        public interface Visitor<R> {
            R visitBlockStmt(Block stmt);
            R visitClassStmt(Class stmt);
            R visitExpressionStmt(Expression stmt);
            R visitFunctionStmt(Function stmt);
            R visitIfStmt(If stmt);
            R visitPrintStmt(Print stmt);
            R visitReturnStmt(Return stmt);
            R visitVarStmt(Var stmt);
            R visitWhileStmt(While stmt);
        }


        public class Block : Stmt {
            public Block(List<Stmt> statements) {
                this.statements = statements;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitBlockStmt(this);
            }

            public readonly List<Stmt> statements;
        }


        public class Class : Stmt {
            public Class(Token name, Expr.Variable superclass, List<Stmt.Function> methods) {
                this.name = name;
                this.superclass = superclass;
                this.methods = methods;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitClassStmt(this);
            }

            public readonly Token name;
            public readonly Expr.Variable superclass;
            public readonly List<Stmt.Function> methods;
        }


        public class Expression : Stmt {
            public Expression(Expr expression) {
                this.expression = expression;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitExpressionStmt(this);
            }

            public readonly Expr expression;
        }


        public class Function : Stmt {
            public Function(Token name, List<Token> prms, List<Stmt> body) {
                this.name = name;
                this.prms = prms;
                this.body = body;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitFunctionStmt(this);
            }

            public readonly Token name;
            public readonly List<Token> prms;
            public readonly List<Stmt> body;
        }


        public class If : Stmt {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitIfStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt thenBranch;
            public readonly Stmt elseBranch;
        }


        public class Print : Stmt {
            public Print(Expr expression) {
                this.expression = expression;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitPrintStmt(this);
            }

            public readonly Expr expression;
        }


        public class Return : Stmt {
            public Return(Token keyword, Expr value) {
                this.keyword = keyword;
                this.value = value;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitReturnStmt(this);
            }

            public readonly Token keyword;
            public readonly Expr value;
        }


        public class Var : Stmt {
            public Var(Token name, Expr initializer) {
                this.name = name;
                this.initializer = initializer;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitVarStmt(this);
            }

            public readonly Token name;
            public readonly Expr initializer;
        }


        public class While : Stmt {
            public While(Expr condition, Stmt body) {
                this.condition = condition;
                this.body = body;
            }

            public override R accept<R>(Visitor<R> visitor) {
                return visitor.visitWhileStmt(this);
            }

            public readonly Expr condition;
            public readonly Stmt body;
        }



        public abstract R accept<R>(Visitor<R> visitor);
    }
}
