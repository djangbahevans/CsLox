using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr) => expr.Accept(this);

        public string VisitBinaryExpr(Expr.Binary expr) => Parethesize(expr.op.lexeme, expr.left, expr.right);

        public string VisitGroupingExpr(Expr.Grouping expr) => Parethesize("group", expr.expression);

        public string VisitLiteralExpr(Expr.Literal expr) => (expr.value == null) ? "null" : expr.value.ToString();

        public string VisitUnaryExpr(Expr.Unary expr) => Parethesize(expr.op.lexeme, expr.right);

        private string Parethesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");
            return builder.ToString();
        }

        //public static void Main(string[] args)
        //{
        //    Expr expression = new Expr.Binary(
        //        new Expr.Unary(
        //            new Token(TokenType.MINUS, "-", null, 1),
        //            new Expr.Literal(123)),
        //        new Token(TokenType.STAR, "*", null, 1),
        //        new Expr.Grouping(new Expr.Literal(45.67)));

        //    Console.WriteLine(new AstPrinter().Print(expression));
        //}
    }
}
