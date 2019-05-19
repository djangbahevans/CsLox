using System.Text;

namespace CsLox
{
    internal class AstPrinter : Expr.IVisitor<string>
    {
        public string Print(Expr expr) => expr.Accept(this);

        public string VisitAssignExpr(Expr.Assign expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBinaryExpr(Expr.Binary expr) => Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
        public string VisitCallExpr(Expr.Call expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGetExpr(Expr.Get expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGroupingExpr(Expr.Grouping expr) => Parenthesize("group", expr.Expression);

        public string VisitLiteralExpr(Expr.Literal expr) => (expr.Value == null) ? "null" : expr.Value.ToString();

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            throw new System.NotImplementedException();
        }

        public string VisitUnaryExpr(Expr.Unary expr) => Parenthesize(expr.Operator.Lexeme, expr.Right);

        public string VisitVariableExpr(Expr.Variable expr)
        {
            throw new System.NotImplementedException();
        }

        private string Parenthesize(string name, params Expr[] exprs)
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