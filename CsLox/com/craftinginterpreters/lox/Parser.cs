using com.craftinginterpreters.lox;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Activation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static com.craftinginterpreters.lox.TokenType;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    internal class Parser
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract class LoxError : Exception
        {
            protected LoxError() : base()
            {
            }

            protected LoxError(string message) : base(message)
            {
            }

            protected LoxError(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ParseError : LoxError { }

        /// <summary>
        /// 
        /// </summary>
        private readonly List<Token> tokens;

        /// <summary>
        /// 
        /// </summary>
        private int current = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        internal Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal List<Stmt> parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr expression()
        {
            return assignment();
        }

        private Stmt declaration()
        {
            try
            {
                if (match(CLASS)) return classDeclaration();
                if (match(FUN)) return function("function");
                if (match(VAR)) return varDeclaration();

                return statement();
            } 
            catch (ParseError error)
            {
                synchronize();
                return null;
            }
        }

        private Stmt classDeclaration()
        {
            Token name = consume(IDENTIFIER, "Expect class name.");

            Expr.Variable superclass = null;
            if (match(LESS))
            {
                consume(IDENTIFIER, "Expect superclass name.");
                superclass = new Expr.Variable(previous());
            }

            consume(LEFT_BRACE, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            while (!check(RIGHT_BRACE) && !isAtEnd())
            {
                methods.Add(function("method"));
            }

            consume(RIGHT_BRACE, "Expect '}' after class body.");

            return new Stmt.Class(name, superclass, methods);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt statement()
        {
            if (match(FOR)) return forStatement();
            if (match(IF)) return ifStatement();
            if (match(PRINT)) return printStatement();
            if (match(RETURN)) return returnStatement();
            if (match(WHILE)) return whileStatement();
            if (match(LEFT_BRACE)) return new Stmt.Block(block());

            return expressionStatement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt forStatement()
        {
            consume(LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (match(SEMICOLON))
            {
                initializer = null;
            }
            else if (match(VAR))
            {
                initializer = varDeclaration();
            }
            else
            {
                initializer = expressionStatement();
            }

            Expr condition = null;
            if (!check(SEMICOLON))
            {
                condition = expression();
            }
            consume(SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!check(RIGHT_PAREN))
            {
                increment = expression();
            }
            consume(RIGHT_PAREN, "Expect ')' after for clauses.");
            Stmt body = statement();

            Stmt[] stmts;
            if (increment != null)
            {
                stmts = new Stmt[] { body, new Stmt.Expression(increment) };
                body = new Stmt.Block(stmts.ToList<Stmt>());
                //Arrays.asList(body, new Stmt.Expression(increment)));
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                stmts = new Stmt[] { initializer, body };
                body = new Stmt.Block(stmts.ToList<Stmt>());
                //Arrays.asList(initializer, body)
            }

            return body;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt ifStatement()
        {
            consume(LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = statement();
            Stmt elseBranch = null;
            if (match(ELSE))
            {
                elseBranch = statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt printStatement()
        {
            Expr value = expression();
            consume(SEMICOLON, "Expect ';' after value.");
            return new Stmt.Print(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            if (!check(SEMICOLON))
            {
                value = expression();
            }

            consume(SEMICOLON, "Expect ';' after return value.");
            return new Stmt.Return(keyword, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt varDeclaration()
        {
            Token name = consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if(match(EQUAL))
            {
                initializer = expression();
            }

            consume(SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initializer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt whileStatement()
        {
            consume(LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = expression();
            consume(RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = statement();

            return new Stmt.While(condition, body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Stmt expressionStatement()
        {
            Expr expr = expression();
            consume(SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        private Stmt.Function function(String kind)
        {
            Token name = consume(IDENTIFIER, "Expect " + kind + " name.");
            consume(LEFT_PAREN, "Expect '(' after " + kind + " name.");
            List<Token> parameters = new List<Token>();
            if (!check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        error(peek(), "Can't have more than 255 parameters.");
                    }

                    parameters.Add(consume(IDENTIFIER, "Expect parameter name."));
                } while (match(COMMA));
            }
            consume(RIGHT_PAREN, "Expect ')' after parameters.");

            consume(LEFT_BRACE, "Expect '{' before " + kind + " body.");
            List<Stmt> body = block();
            return new Stmt.Function(name, parameters, body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!check(RIGHT_BRACE) && !isAtEnd())
            {
                statements.Add(declaration());
            }

            consume(RIGHT_BRACE, "Expect '}' at the end of a block.");
            return statements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr assignment()
        {
            Expr expr = or();

            if (match(EQUAL))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }
                else if (expr is Expr.Get)
                {
                    Expr.Get get = (Expr.Get)expr;
                    return new Expr.Set(get.obj, get.name, value);
                }

                error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr or()
        {
            Expr expr = and();

            while (match(OR))
            {
                Token oprtr = previous();
                Expr right = and();
                expr = new Expr.Logical(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr and()
        {
            Expr expr = equality();

            while (match(AND))
            {
                Token oprtr = previous();
                Expr right = equality();
                expr = new Expr.Logical(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr equality()
        {
            Expr expr = comparison();

            while(match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token oprtr = previous();
                Expr right = comparison();
                expr = new Expr.Binary(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr comparison()
        {
            Expr expr = term();

            while(match(new TokenType[] { GREATER, GREATER_EQUAL, LESS, LESS_EQUAL }))
            {
                Token oprtr = previous();
                Expr right = term();
                expr = new Expr.Binary(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr term()
        {
            Expr expr = factor();

            while(match(MINUS, PLUS))
            {
                Token oprtr = previous();
                Expr right = factor();
                expr = new Expr.Binary(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr factor()
        {
            Expr expr = unary();

            while (match(SLASH, STAR)) {
                Token oprtr = previous();
                Expr right = unary();
                expr = new Expr.Binary(expr, oprtr, right);
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr unary()
        {
            if(match(BANG, MINUS))
            {
                Token oprtr = previous();
                Expr right = unary();
                return new Expr.Unary(oprtr, right);
            }

            return call();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callee"></param>
        /// <returns></returns>
        private Expr finishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        error(peek(), "Can't have more than 255 arguments.");
                    }
                    arguments.Add(expression());
                } while (match(COMMA));
            }

            Token paren = consume(RIGHT_PAREN, "Expect ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr call()
        {
            Expr expr = primary();

            while (true)
            {
                if (match(LEFT_PAREN))
                {
                    expr = finishCall(expr);
                }
                else if (match(DOT))
                {
                    Token name = consume(IDENTIFIER, "Expect property name after '.'.");
                    expr = new Expr.Get(expr, name);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Expr primary()
        {
            if (match(FALSE))
            {
                //Lox.log(0, "Parser.primary ZZZZ 000");
                return new Expr.Literal(false);
            }

            if (match(TRUE))
            {
                //Lox.log(0, "Parser.primary ZZZZ 111");
                return new Expr.Literal(true);
            }

            if (match(NIL))
            {
                //Lox.log(0, "Parser.primary ZZZZ 222");
                return new Expr.Literal(null);
            }
        
            if(match(NUMBER, STRING))
            {
                //Lox.log(0, "Parser.primary ZZZZ 333");
                return new Expr.Literal(previous().literal);
            }

            if (match(SUPER))
            {
                Token keyword = previous();
                consume(DOT, "Expect '.' after 'super'.");
                Token method = consume(IDENTIFIER, "Expect superclass method name.");
                return new Expr.Super(keyword, method);
            }

            if (match(THIS)) return new Expr.This(previous());

            if (match(IDENTIFIER))
            {
                //Lox.log(0, "Parser.primary ZZZZ 444");
                return new Expr.Variable(previous());
            }

            if (match(LEFT_PAREN))
            {
                Expr expr = expression();
                consume(RIGHT_PAREN, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw error(peek(), "Expect expression.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private bool match(TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if(check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        private bool match(TokenType type1, TokenType type2)
        {
            if (check(type1))
            {
                advance();
                return true;
            }

            if (check(type2))
            {
                advance();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool match(TokenType type)
        {
            if (check(type))
            {
                advance();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private Token consume(TokenType type, String message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        /// <summary>
        /// 
        /// </summary>
        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool isAtEnd()
        {
            return peek().type == EOF;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Token peek()
        {
            return tokens.ElementAt(current);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Token previous()
        {
            return tokens.ElementAt(current - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private ParseError error(Token token, String message)
        {
            Lox.error(token, message);
            return new ParseError();
        }

        /// <summary>
        /// 
        /// </summary>
        private void synchronize()
        {
            advance();

            while (!isAtEnd())
            {
                if (previous().type == SEMICOLON) return;

                switch(peek().type)
                {
                    case CLASS:
                    case FUN:
                    case VAR:
                    case FOR:
                    case IF:
                    case WHILE:
                    case PRINT:
                    case RETURN:
                        return;
                }

                advance();
            }
        }
    }
}
