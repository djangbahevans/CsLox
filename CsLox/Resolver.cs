using System;
using System.Collections.Generic;
using System.Linq;

namespace CsLox
{
    internal class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<IDictionary<string, bool>> _scopes = new Stack<IDictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.Callee);
            foreach (Expr argument in expr.Arguments) Resolve(argument);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.Expression_);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.Expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
            if (stmt.Value != null) Resolve(stmt.Value);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool getValue);
            if (_scopes.Count != 0 && getValue == false)
                Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null) Resolve(stmt.Initializer);
            Define(stmt.Name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);
            return null;
        }

        internal void Resolve(IEnumerable<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        private void BeginScope()
        {
            _scopes.Push((new Dictionary<string, bool>()));
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0) return;

            IDictionary<string, bool> scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
                Lox.Error(name, "Variable with this name already declared in this scope.");
            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0) return;
            _scopes.Peek().Add(name.Lexeme, true);
        }

        private void EndScope()
        {
            _scopes.Pop();
        }
        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;
            BeginScope();
            foreach (Token parameter in function.Parameters)
            {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(function.Body);
            EndScope();
            currentFunction = enclosingFunction;
        }
        private void ResolveLocal(Expr expr, Token name)
        {
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                if (!_scopes.ElementAt(i).ContainsKey(name.Lexeme)) continue;
                _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                return;
            }
        }
        private enum FunctionType
        {
            NONE,
            FUNCTION
        }
    }
}
