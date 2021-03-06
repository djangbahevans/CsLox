﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static CsLox.TokenType;

namespace CsLox
{
    internal class Parser
    {
        /// <summary>
        /// Tokens to parse.
        /// </summary>
        private readonly List<Token> _tokens;

        /// <summary>
        /// Current token being parsed.
        /// </summary>
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            this._tokens = tokens;
        }

        /// <summary>
        /// Parses functions and methods
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>A new function statement</returns>
        private Stmt.Function Function(string kind)
        {
            Token name = null;
            if (Check(IDENTIFIER))
                name = Consume(IDENTIFIER, $"Expect {kind} name.");
            Consume(LEFT_PAREN, $"Expected '(' after {kind}");
            List<Token> parameters = new List<Token>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (parameters.Count() > 8) Error(Peek(), "Cannot have more than 8 parameters");
                    parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
                } while (Match(COMMA));
            }

            Consume(RIGHT_PAREN, "Expect ')' after parameters");

            Consume(LEFT_BRACE, "Expect '{' before body");
            List<Stmt> body = Block();
            return new Stmt.Function(name, parameters, body);
        }

        /// <summary>
        /// Parses source code and return a list of statements
        /// </summary>
        /// <returns>List of statements</returns>
        internal List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        /// <summary>
        /// Reports an error to the Lox environment and returns a ParseError object.
        /// </summary>
        /// <param name="token">The token that generated the error</param>
        /// <param name="message">The error message to report</param>
        /// <returns>ParseError object</returns>
        private static ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(MINUS, PLUS))
            {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(AND))
            {
                Token op = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (!Match(EQUAL)) return expr;
            Token equals = Previous();
            Expr value = Assignment();

            switch (expr)
            {
                case Expr.Variable variable:
                    return new Expr.Assign(variable.Name, value);
                case Expr.Get get:
                    return new Expr.Set(get.Object, get.Name, value);
                default:
                    Error(@equals, "Invalid assignment target.");

                    return expr;
            }
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(RIGHT_BRACE) && !IsAtEnd()) statements.Add(Declaration());
            Consume(RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Call()
        {

            Expr expr = Primary();

            while (true)
            {
                if (Match(LEFT_PAREN)) expr = FinishCall(expr);
                else if (Match(DOT))
                {
                    Token name = Consume(IDENTIFIER, "Expect property name after '.'");
                    expr = new Expr.Get(expr, name);
                }
                else break;
            }

            return expr;
        }

        /// <summary>
        /// Checks if current token is equal to input TokenType
        /// </summary>
        /// <param name="type">TokenType to compare</param>
        /// <returns></returns>
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Stmt ClassDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect class name.");

            Expr.Variable superclass = null;
            if (Match(LESS))
            {
                Consume(IDENTIFIER, "Expect superclass name");
                superclass = new Expr.Variable(Previous());
            }
            Consume(LEFT_BRACE, "Expect '{' before class body.");

            List<Stmt.Function> methods = new List<Stmt.Function>();
            while (!Check(RIGHT_BRACE) && !IsAtEnd()) methods.Add(Function("method"));

            Consume(RIGHT_BRACE, "Expect '}' after class body");

            return new Stmt.Class(name, superclass, methods);
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
            {
                Token op = Previous();
                Expr right = Addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(CLASS)) return ClassDeclaration();
                if (Match(FUN)) return Function("function");
                if (Match(VAR)) return VarDeclaration();
                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }
        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(BANG_EQUAL, EQUAL_EQUAL))
            {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }
        private Expr Expression() => Assignment();

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(SEMICOLON, "Expected ';' after value.");
            return new Stmt.Expression(expr);
        }

        private Expr FinishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();
            if (!Check(RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= 8) Error(Peek(), "Cannot have more than 8 arguments");
                    //if (Match(FUN)) arguments.Add(Function("function"));
                    //else arguments.Add(Expression());
                    arguments.Add(Expression());
                } while (Match(COMMA));
            }

            Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments");

            return new Expr.Call(callee, paren, arguments);
        }

        private Stmt ForStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(SEMICOLON)) initializer = null;
            else if (Match(VAR)) initializer = VarDeclaration();
            else initializer = ExpressionStatement();

            Expr condition = null;
            if (!Check(SEMICOLON)) condition = Expression();
            Consume(SEMICOLON, "Expect ';' after loop condition");

            Expr increment = null;
            if (!Check(SEMICOLON)) increment = Expression();
            Consume(RIGHT_PAREN, "Expect ';' after for clauses");

            Stmt body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>()
                {
                    body,
                    new Stmt.Expression(increment)
                });
            }

            if (condition == null) condition = new Expr.Literal(true);

            body = new Stmt.While(condition, body);

            if (initializer != null) body = new Stmt.Block(new List<Stmt>() { initializer, body });

            return body;
        }

        private Stmt IfStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after if.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;
            if (Match(ELSE)) elseBranch = Statement();
            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private bool IsAtEnd() => Peek().Type == EOF;

        private bool Match(params TokenType[] types)
        {
            if (!types.Any(Check)) return false;
            Advance();
            return true;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(SLASH, STAR))
            {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(OR))
            {
                Token op = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        [SuppressMessage("ReSharper", "InvertIf")]
        private Expr Primary()
        {
            if (Match(FALSE)) return new Expr.Literal(false);
            if (Match(TRUE)) return new Expr.Literal(true);
            if (Match(NIL)) return new Expr.Literal(null);

            if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);

            if (Match(THIS)) return new Expr.This(Previous());

            if (Match(SUPER))
            {
                Token keyword = Previous();
                Consume(DOT, "Expect '.' after 'super'.");
                Token method = Consume(IDENTIFIER, "Expect superclass method name.");
                return new Expr.Super(keyword, method);
            }

            if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(RIGHT_PAREN, "Expected ')' after expression");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected ')' after expression");
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(SEMICOLON, "Expected ';' after value.");
            return new Stmt.Print(value);
        }

        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(SEMICOLON)) value = Expression();

            Consume(SEMICOLON, "Expect ';' after return value");
            return new Stmt.Return(keyword, value);
        }

        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        private Stmt Statement()
        {
            if (Match(FOR)) return ForStatement();
            if (Match(IF)) return IfStatement();
            if (Match(PRINT)) return PrintStatement();
            if (Match(RETURN)) return ReturnStatement();
            if (Match(WHILE)) return WhileStatement();
            if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }
        private void Synchronize()
        {
            Advance();

            while (IsAtEnd())
            {
                if (Previous().Type == SEMICOLON) return;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (Peek().Type)
                {
                    case CLASS:
                    case FUN:
                    case FOR:
                    case IF:
                    case PRINT:
                    case RETURN:
                    case VAR:
                    case WHILE:
                        return;
                }

                Advance();
            }
        }

        /// <summary>
        /// Parses Unary operations
        /// </summary>
        /// <returns></returns>
        private Expr Unary()
        {
            if (!Match(BANG, MINUS)) return Call();
            Token op = Previous();
            Expr right = Unary();
            return new Expr.Unary(op, right);
        }

        /// <summary>
        /// Parses var declarations
        /// </summary>
        /// <returns></returns>
        private Stmt VarDeclaration()
        {
            Token name = Consume(IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(EQUAL)) initializer = Expression();

            Consume(SEMICOLON, "Expect ';' after variable initialization.");
            return new Stmt.Var(name, initializer);
        }

        /// <summary>
        /// Parses 'while' statements
        /// </summary>
        /// <returns></returns>
        private Stmt WhileStatement()
        {
            Consume(LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        private class ParseError : SystemException
        {
        }
    }
}