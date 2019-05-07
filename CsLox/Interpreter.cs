using System;
using System.Collections.Generic;
using static CsLox.TokenType;

namespace CsLox
{
    class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private Environment environment = new Environment();

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

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        private string Stringify(object value)
        {
            if (value == null) return "nil";
            if (value is double)
            {
                string text = value.ToString();
                if (text.EndsWith(".0"))
                    text = text.Substring(0, text.Length - 2);
                return text;
            }
            return value.ToString();
        }

        private object Evaluate(Expr expression)
        {
            return expression.Accept(this);
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.Type)
            {
                case GREATER:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left > (double)right;
                case GREATER_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left >= (double)right;
                case LESS:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left < (double)right;
                case LESS_EQUAL:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left <= (double)right;
                case BANG_EQUAL:
                    return !IsEqual(left, right);
                case EQUAL_EQUAL:
                    return IsEqual(left, right);
                case MINUS:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left - (double)right;
                case SLASH:
                    CheckNumberOperand(expr.op, left, right);
                    if ((double)right == 0) throw new RuntimeError(expr.op, "Cannot divide by zero.");
                    return (double)left / (double)right;
                case STAR:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    if (left is string || right is string)
                        return left.ToString() + right.ToString();

                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number");
        }

        private void CheckNumberOperand(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers");
        }

        private bool IsTruthy(object right)
        {
            if (right == null) return false;
            if (right is bool) return (bool)right;

            return true;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.Type)
            {
                case MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
                case BANG:
                    return !IsTruthy(right);
            }

            return null;
        }

        object Stmt.IVisitor<object>.VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        object Stmt.IVisitor<object>.VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null) value = Evaluate(stmt.initializer);
            environment.Define(stmt.name.Lexeme, value);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return environment.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);

            environment.Assign(expr.name, value);
            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        private void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }
    }
}
