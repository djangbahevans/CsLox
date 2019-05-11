using System.Collections.Generic;
using static CsLox.TokenType;

namespace CsLox
{
    internal class Scanner
    {
        /// <summary>
        /// Holds all the keywords in Lox.
        /// </summary>
        private readonly Dictionary<string, TokenType> _keywords;
        /// <summary>
        /// Source text to scan
        /// </summary>
        private readonly string _source;
        /// <summary>
        /// List of all currently identified valid Tokens.
        /// </summary>
        private readonly List<Token> _tokens = new List<Token>();
        /// <summary>
        /// Current text position of Token being scanned.
        /// </summary>
        private int _current = 0;
        /// <summary>
        /// Current line being scan
        /// </summary>
        private int _line = 1;
        /// <summary>
        /// Start position of Token being scanned.
        /// </summary>
        private int _start = 0;
        public Scanner(string source)
        {
            _keywords = new Dictionary<string, TokenType>
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

            _source = source;
        }

        /// <summary>
        /// Scans text for Tokens
        /// </summary>
        /// <returns></returns>
        internal List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(EOF, "", null, _line));
            return _tokens;
        }

        /// <summary>
        /// Determines if character is an alphabet or _
        /// </summary>
        /// <param name="c">Character to scan</param>
        /// <returns></returns>
        private static bool IsAlpha(char c) => char.IsLetter(c) || (c == '_');

        /// <summary>
        /// Determines if character is alphanumeric or _
        /// </summary>
        /// <param name="c">Character to scan</param>
        /// <returns></returns>
        private static bool IsAlphaNumeric(char c) => IsAlpha(c) || char.IsDigit(c);

        /// <summary>
        /// Adds Token to Token list.
        /// </summary>
        /// <param name="type">Type of Token to add</param>
        /// <param name="literal">Literal attached to the Token, defaults to null.</param>
        private void AddToken(TokenType type, object literal = null)
        {
            string text = _source.Substring(_start, _current - _start);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        /// <summary>
        /// Advances to next character in source text.
        /// </summary>
        /// <returns>Returns previous character in source text</returns>
        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }

        /// <summary>
        /// Scans an Identifier and adds to Token list.
        /// </summary>
        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            // See if Identifier is a keyword
            string text = _source.Substring(_start, _current - _start);
            _keywords.TryGetValue(text, out TokenType type);
            if (type == LEFT_PAREN) type = IDENTIFIER;
            AddToken(type);
        }

        /// <summary>
        /// Determines if scanning has reached the end of the source text.
        /// </summary>
        /// <returns></returns>
        private bool IsAtEnd() => _current >= _source.Length;

        /// <summary>
        /// Determines if current character matches an expected character. Advances when true.
        /// </summary>
        /// <param name="expected">Character expected</param>
        /// <returns></returns>
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;

            _current++;
            return true;
        }

        /// <summary>
        /// Scans a number and adds to Token list
        /// </summary>
        private void Number()
        {
            while (char.IsDigit(Peek())) Advance();

            if (Peek() == '.' && char.IsDigit(PeekNext()))
            {
                Advance();
                while (char.IsDigit(Peek())) Advance();
            }
            AddToken(NUMBER, double.Parse(_source.Substring(_start, _current - _start)));
        }

        /// <summary>
        /// Gets the current character in source text.
        /// </summary>
        /// <returns>Returns the current character</returns>
        private char Peek() => IsAtEnd() ? '\0' : _source[_current];

        /// <summary>
        /// Gets the next character in source text.
        /// </summary>
        /// <returns>Return the next character</returns>
        private char PeekNext() => _current + 1 >= _source.Length ? '\0' : _source[_current + 1];

        /// <summary>
        /// Initiates the scanning of tokens based on the first character.
        /// </summary>
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
                            else if (Peek() == '\n') _line++;
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
                    _line++;
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
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        /// <summary>
        /// Scans a string and adds it to Token list
        /// </summary>
        private void Text()
        {
            while (Peek() != '"' & !IsAtEnd()) // Keep adding until we hit a closing "
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            if (IsAtEnd()) // Return an error if we do not hit a closing "
            {
                Lox.Error(_line, "Unterminated string.");
            }

            Advance();

            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(STRING, value);
        }
    }
}