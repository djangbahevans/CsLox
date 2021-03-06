// This is an autogenerated file. Do not change.

using System;
using System.Collections.Generic;

namespace CsLox
{
    internal abstract class Expr
    {
        public interface IVisitor<T>
        {
            T VisitAssignExpr(Assign expr);
            T VisitBinaryExpr(Binary expr);
            T VisitCallExpr(Call expr);
            T VisitGetExpr(Get expr);
            T VisitGroupingExpr(Grouping expr);
            T VisitLiteralExpr(Literal expr);
            T VisitLogicalExpr(Logical expr);
            T VisitSetExpr(Set expr);
            T VisitSuperExpr(Super expr);
            T VisitThisExpr(This expr);
            T VisitUnaryExpr(Unary expr);
            T VisitVariableExpr(Variable expr);
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                this.Name = name;
                this.Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public Token Name { get; }
            public Expr Value { get; }
        }

        public class Binary : Expr
        {
            public Binary(Expr left, Token @operator, Expr right)
            {
                this.Left = left;
                this.Operator = @operator;
                this.Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }
        }

        public class Call : Expr
        {
            public Call(Expr callee, Token paren, IEnumerable<Expr> arguments)
            {
                this.Callee = callee;
                this.Paren = paren;
                this.Arguments = arguments;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitCallExpr(this);
            }

            public Expr Callee { get; }
            public Token Paren { get; }
            public IEnumerable<Expr> Arguments { get; }
        }

        public class Get : Expr
        {
            public Get(Expr @object, Token name)
            {
                this.Object = @object;
                this.Name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGetExpr(this);
            }

            public Expr Object { get; }
            public Token Name { get; }
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                this.Expression = expression;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr Expression { get; }
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                this.Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public object Value { get; }
        }

        public class Logical : Expr
        {
            public Logical(Expr left, Token @operator, Expr right)
            {
                this.Left = left;
                this.Operator = @operator;
                this.Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public Expr Left { get; }
            public Token Operator { get; }
            public Expr Right { get; }
        }

        public class Set : Expr
        {
            public Set(Expr @object, Token name, Expr value)
            {
                this.Object = @object;
                this.Name = name;
                this.Value = value;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSetExpr(this);
            }

            public Expr Object { get; }
            public Token Name { get; }
            public Expr Value { get; }
        }

        public class Super : Expr
        {
            public Super(Token keyword, Token method)
            {
                this.Keyword = keyword;
                this.Method = method;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSuperExpr(this);
            }

            public Token Keyword { get; }
            public Token Method { get; }
        }

        public class This : Expr
        {
            public This(Token keyword)
            {
                this.Keyword = keyword;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitThisExpr(this);
            }

            public Token Keyword { get; }
        }

        public class Unary : Expr
        {
            public Unary(Token @operator, Expr right)
            {
                this.Operator = @operator;
                this.Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token Operator { get; }
            public Expr Right { get; }
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.Name = name;
            }

            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public Token Name { get; }
        }

        public abstract T Accept<T>(IVisitor<T> visitor);
    }
}
