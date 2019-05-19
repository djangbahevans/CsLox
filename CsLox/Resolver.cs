using System.Collections.Generic;
using System.Linq;

namespace CsLox
{
    internal class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<IDictionary<string, bool>> _scopes = new Stack<IDictionary<string, bool>>();
        private ClassType _currentClass = ClassType.NONE;
        private FunctionType _currentFunction = FunctionType.NONE;
        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
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

        public object VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(stmt.Name);
            Define(stmt.Name);

            BeginScope();
            _scopes.Peek().Add("this", true);

            foreach (Stmt.Function method in stmt.Methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.Name.Lexeme.Equals("init")) declaration = FunctionType.INITIALIZER;
                ResolveFunction(method, declaration);
            }

            EndScope();

            _currentClass = enclosingClass;
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

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.Object);
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
            if (_currentFunction == FunctionType.NONE)
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
            if (stmt.Value != null)
            {
                if (_currentFunction == FunctionType.INITIALIZER)
                    Lox.Error(stmt.Keyword, "Cannot return a value from an initializer.");
                Resolve(stmt.Value);
            }
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.Value);
            Resolve(expr.Object);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                Lox.Error(expr.Keyword, "Cannot use 'this' outside of a class.");
            }
            ResolveLocal(expr, expr.Keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            _scopes.TryPeek(out IDictionary<string, bool> peekedDictionary);
            bool getValue = false;
            peekedDictionary?.TryGetValue(expr.Name.Lexeme, out getValue);
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
            _scopes.Peek()[name.Lexeme] = true;
            //_scopes.Peek().Add(name.Lexeme, true);
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
            FunctionType enclosingFunction = _currentFunction;
            _currentFunction = type;
            BeginScope();
            foreach (Token parameter in function.Parameters)
            {
                Declare(parameter);
                Define(parameter);
            }
            Resolve(function.Body);
            EndScope();
            _currentFunction = enclosingFunction;
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
    }
}
