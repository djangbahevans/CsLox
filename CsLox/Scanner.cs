using System;
using System.Collections.Generic;
using static CsLox.TokenType;

namespace CsLox
{
    internal class Scanner
    {
        private readonly Dictionary<string, TokenType> keywords;
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            keywords = new Dictionary<string, TokenType>
            {
                { "and", AND },
                { "class", CLASS },
                { "else", ELSE },
                { "false", FALSE },
                { "for", FOR },
                { "fun", FUN },
                { "if", IF },
                { "nil", NIL },
                { "or", OR },
                { "print", PRINT },
                { "return", RETURN },
                { "super", SUPER },
                { "this", THIS },
                { "true", TRUE },
                { "var", VAR },
                { "while", WHILE }
            };

            this.source = source;
        }

        internal List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(':
                    AddToken(LEFT_PAREN);
                    break;
                case ')':
                    AddToken(RIGHT_PAREN);
                    break;
                case '{':
                    AddToken(LEFT_BRACE);
                    break;
                case '}':
                    AddToken(RIGHT_BRACE);
                    break;
                case ',':
                    AddToken(COMMA);
                    break;
                case '.':
                    AddToken(DOT);
                    break;
                case '-':
                    AddToken(MINUS);
                    break;
                case '+':
                    AddToken(PLUS);
                    break;
                case ';':
                    AddToken(SEMICOLON);
                    break;
                case '*':
                    AddToken(STAR);
                    break;
                case '!':
                    AddToken(Match('=') ? BANG_EQUAL : BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? LESS_EQUAL : LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else if (Match('*'))
                    {
                        // A multiline comment until we reach */
                        while (!IsAtEnd())
                        {
                            if (Match('*'))
                            {
                                if (Match('/')) break; // Closing */ reached
                            }
                            else if (Peek() == '\n') line++;
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;
                case '"':
                    Text();
                    break;

                default:
                    if (char.IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void Text()
        {
            while (Peek() != '"' & !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
            }

            Advance();

            string value = source.Substring(start + 1, current - start - 2);
            AddToken(STRING, value);
        }

        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool IsAlpha(char c)
        {
            return char.IsLetter(c) || (c == '_');
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            AddToken(IDENTIFIER);
        }

        private bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);

        private void Number()
        {
            while (char.IsDigit(Peek())) Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(Peek())) Advance();
            }
            AddToken(NUMBER, Double.Parse(source.Substring(start, current - start)));
        }

        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private char Advance()
        {
            current++;
            return source[current - 1];
        }

        private bool IsAtEnd() => current >= source.Length;
    }
}