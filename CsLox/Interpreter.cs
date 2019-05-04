using System;
using static CsLox.TokenType;

namespace CsLox
{
    class Interpreter : Expr.IVisitor<object>
    {
        internal void Interpret(Expr expression)
        {
            try
            {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            }
            catch (RuntimeError error)
            {

                Lox.RuntimeError(error);
            }
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
                    return (double)left / (double)right;
                case STAR:
                    CheckNumberOperand(expr.op, left, right);
                    return (double)left * (double)right;
                case PLUS:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    if (left is string && right is string)
                        return (string)left + (string)right;

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
    }
}
