using System;
using System.Collections.Generic;
using System.Linq;
using static CsLox.TokenType;

namespace CsLox
{
    internal class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private Environment _environment = new Environment();
        private readonly IDictionary<Expr, int?> _locals = new Dictionary<Expr, int?>();

        public Interpreter()
        {
            Globals.Define("clock", new TempLoxCallable());
        }

        internal Environment Globals { get; } = new Environment();

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.Value);

            int? distance = _locals[expr];
            if (distance != null)
            {
                _environment.AssignAt((int)distance, expr.Name, value);
            }
            else
            {
                Globals.Assign(expr.Name, value);
            }
            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expr.Op.Type)
            {
                case GREATER:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left <= (double)right;
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
                case MINUS:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    CheckNumberOperand(expr.Op, left, right);
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if ((double)right == 0) throw new RuntimeError(expr.Op, "Cannot divide by zero.");
                    return (double)left / (double)right;
                case STAR:
                    CheckNumberOperand(expr.Op, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double d && right is double d1)
                        return d + d1;
                    if (left is string || right is string)
                        return left.ToString() + right;

                    throw new RuntimeError(expr.Op, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_environment));
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.Callee);

            List<object> arguments = expr.Arguments.Select(Evaluate).ToList();

            if (!(callee is ILoxCallable)) throw new RuntimeError(expr.Paren, "Can only call functions and classes.");

            ILoxCallable function = (ILoxCallable)callee;
            if (arguments.Count() != function.Arity())
            {
                throw new RuntimeError(expr.Paren, $"Expect {function.Arity()} arguments but got {arguments.Count()} instead.");
            }
            return function.Call(this, arguments);
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.Expression_);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, _environment);
            if (stmt.Name != null)
                _environment.Define(stmt.Name.Lexeme, function);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.Left);

            if (expr.Op.Type == OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.Right);
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.Expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.Value != null) value = Evaluate(stmt.Value);

            throw new Return(value);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.Right);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (expr.Op.Type)
            {
                case MINUS:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
                case BANG:
                    return !IsTruthy(right);
            }

            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookupVariable(expr.Name, expr);
        }

        private object LookupVariable(Token name, Expr expr)
        {
            _locals.TryGetValue(expr, out int? distance);
            return distance != null ? _environment.GetAt((int)distance, name.Lexeme) : Globals.Get(name);
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.Initializer != null) value = Evaluate(stmt.Initializer);
            _environment.Define(stmt.Name.Lexeme, value);
            return null;
        }
        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }
            return null;
        }

        internal void ExecuteBlock(IEnumerable<Stmt> statements, Environment environment)
        {
            Environment previous = this._environment;
            try
            {
                this._environment = environment;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this._environment = previous;
            }
        }

        internal void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {

                Lox.RuntimeError(error);
            }
        }

        private static void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number");
        }

        private static void CheckNumberOperand(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers");
        }

        private static bool IsEqual(object left, object right)
        {
            switch (left)
            {
                case null when right == null:
                    return true;
                case null:
                    return false;
                default:
                    return left.Equals(right);
            }
        }

        private static bool IsTruthy(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case bool b:
                    return b;
                default:
                    return true;
            }
        }

        private static string Stringify(object value)
        {
            switch (value)
            {
                case null:
                    return "nil";
                case double _:
                    {
                        string text = value.ToString();
                        if (text.EndsWith(".0"))
                            text = text.Substring(0, text.Length - 2);
                        return text;
                    }

                default:
                    return value.ToString();
            }
        }

        private object Evaluate(Expr expression)
        {
            return expression.Accept(this);
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        private class TempLoxCallable : ILoxCallable
        {
            public int Arity() => 0;

            public object Call(Interpreter interpreter, List<object> arguments) =>
                (double)System.Environment.TickCount / 1000;

            public override string ToString() => "<native fun>";
        }

        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }
    }
}
