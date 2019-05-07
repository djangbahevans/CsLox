namespace CsLox
{
    class Token
    {
        public Token(TokenType type, string lexeme, object literal, int line)
        {
            this.Type = type;
            this.Lexeme = lexeme;
            this.Literal = literal;
            this.Line = line;
        }

        public int Line { get; }

        public object Literal { get; }

        public string Lexeme { get; }

        public TokenType Type { get; }

        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
}
