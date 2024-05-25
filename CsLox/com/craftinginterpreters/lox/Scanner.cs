using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static com.craftinginterpreters.lox.TokenType;

namespace com.craftinginterpreters.lox
{
    /// <summary>
    /// 
    /// </summary>
    internal class Scanner
    {
        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<String, TokenType> keywords;

        /// <summary>
        /// 
        /// </summary>
        static Scanner()
        {
            keywords = new Dictionary<String, TokenType>();
            keywords.Add("and", AND);
            keywords.Add("class", CLASS);
            keywords.Add("else", ELSE);
            keywords.Add("false", FALSE);
            keywords.Add("for", FOR);
            keywords.Add("fun", FUN);
            keywords.Add("if", IF);
            keywords.Add("nil", NIL);
            keywords.Add("null", NIL);
            keywords.Add("or", OR);
            keywords.Add("print", PRINT);
            keywords.Add("return", RETURN);
            keywords.Add("super", SUPER);
            keywords.Add("this", THIS);
            keywords.Add("true", TRUE);
            keywords.Add("var", VAR);
            keywords.Add("while", WHILE);
        }

        /// <summary>
        /// 
        /// </summary>
        private String source;

        /// <summary>
        /// 
        /// </summary>
        private List<Token> tokens = new List<Token>();

        /// <summary>
        /// 
        /// </summary>
        private int start = 0;

        /// <summary>
        /// 
        /// </summary>
        private int current = 0;

        /// <summary>
        /// 
        /// </summary>
        private int line = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        internal Scanner(String source) { 
            this.source = source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal List<Token> scanTokens()
        {
            while(!isAtEnd())
            {
                start = current;
                scanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        /// <summary>
        /// 
        /// </summary>
        private void scanToken()
        {
            char c = advance();
            switch (c)
            {
                case '(': addToken(LEFT_PAREN); break;
                case ')': addToken(RIGHT_PAREN); break;
                case '{': addToken(LEFT_BRACE); break;
                case '}': addToken(RIGHT_BRACE); break;
                case ',': addToken(COMMA); break;
                case '.': addToken(DOT); break;
                case '-': addToken(MINUS); break;
                case '+': addToken(PLUS); break;
                case ';': addToken(SEMICOLON); break;
                case '*': addToken(STAR); break;
                case '!':
                    addToken(match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    addToken(match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    addToken(match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    addToken(match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case '/':
                    if (match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (peek() != '\n' && !isAtEnd()) advance();
                    }
                    else if(match('*'))
                    {
                        // A multi-line comment goes until a '*/' is encountered
                        // Doesn't support multi-line comments inside a multi-line comment
                        //while ((peek() != '*' && peekNext() != '/') && !isAtEnd()) advance();

                        // Supports multi-line comments inside a multi-line comment               
                        int stillOpened = 1;
                        while (stillOpened > 0 && !isAtEnd())
                        {
                            if (peek() == '/' && peekNext() == '*')
                            {
                                stillOpened++;
                                advance();
                                advance();
                            }
                            else if (peek() == '*' && peekNext() == '/')
                            {
                                stillOpened--;
                                advance();
                                advance();
                            }
                            else
                            {
                                advance();
                            }
                        }

                        if(stillOpened != 0) Lox.error(line, "Expected ending multiline comment '*/'.");
                    }
                    else
                    {
                        addToken(SLASH);
                    }
                    break;

                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;

                case '"': loadString(); break;

                default:
                    if (isDigit(c))
                    {
                        loadNumber();
                    }
                    else if (isAlpha(c))
                    {
                        identifier();
                    }
                    else
                    {
                        Lox.error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char advance()
        {
            return source.ToCharArray()[current++];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        private void addToken(TokenType type)
        {
            addToken(type, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="literal"></param>
        private void addToken(TokenType type, Object literal)
        {
            int s = start;
            int e = (current - start);
            String text = source.Substring(s, e);
            //Lox.log(line, "(addToken) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
            tokens.Add(new Token(type, text, literal, line));
        }

        /// <summary>
        /// 
        /// </summary>
        private void identifier()
        {
            while (isAlphaNumeric(peek())) advance();

            int s = start;
            int e = (current - start);
            String text = source.Substring(s, e);
            //Lox.log(line, "(identifier) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
            TokenType type = IDENTIFIER;
            if (keywords.ContainsKey(text))
            {
                type = keywords[text];
            }
            addToken(type);
        }

        /// <summary>
        /// 
        /// </summary>
        private void loadNumber()
        {
            while (isDigit(peek())) advance();

            // Look for a fractional part.
            if(peek() == '.' && isDigit(peekNext()))
            {
                // Consume the "."
                advance();

                while (isDigit(peek())) advance();
            }

            int s = start;
            int e = (current - start);
            String text = source.Substring(s, e);
            //Lox.log(line, "(loadNumber) Found text '" + text + "' with substring offset start '" + s + "' and end '" + e + "'.");
            addToken(NUMBER, Double.Parse(text));
        }

        /// <summary>
        /// 
        /// </summary>
        private void loadString()
        {
            while(peek() != '"' && !isAtEnd())
            {
                if (peek() == '\n') line++;
                advance();
            }

            if(isAtEnd())
            {
                Lox.error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            advance();

            // Trim the surrounding quotes.
            // Handle unescape sequences here.
            int s = start + 1;
            int e = ((current - 1) - (start + 1));
            String value = source.Substring(s, e);
            //Lox.log(line, "(loadString) Found text '" + value + "' with substring offset start '" + s + "' and end '" + e + "'.");
            addToken(STRING, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expected"></param>
        /// <returns></returns>
        private bool match(char expected)
        {
            if (isAtEnd()) return false;
            if (source.ToCharArray()[current] != expected) return false;

            current++;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char peek()
        {
            if (isAtEnd()) return '\0';
            return source.ToCharArray()[current];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char peekNext()
        {
            if(current + 1 >= source.Length) return '\0';
            return source.ToCharArray()[current + 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isAlpha(char c)
        {
            return (
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                c == '_'
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isAlphaNumeric(char c)
        {
            return (isAlpha(c) || isDigit(c));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}
