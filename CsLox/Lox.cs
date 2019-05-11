using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    public class Lox
    {
        private static readonly Interpreter Interpreter = new Interpreter();
        /// <summary>
        /// Flag to trigger if scanning error or parsing error occurs.
        /// </summary>
        private static bool _hadError;
        /// <summary>
        /// Flag to trigger if runtime errors occur.
        /// </summary>
        private static bool _hadRuntimeError = false;

        public static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: CsLox [script]");
                return 64;
            }

            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
            return 0;
        }

        /// <summary>
        /// Constructs error string and passes it to Report()
        /// </summary>
        /// <param name="line">Line error occured on</param>
        /// <param name="message">Message attached to error</param>
        internal static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        /// <summary>
        /// Constructs error string and passes it to Report()
        /// </summary>
        /// <param name="token">Token that caused error</param>
        /// <param name="message">Message attached to error</param>
        internal static void Error(Token token, string message) =>
            Report(token.Line, token.Type == TokenType.EOF ? "at end" : $" at '{token.Lexeme}'", message);

        /// <summary>
        /// Reports runtime errors to stdout
        /// </summary>
        /// <param name="error">Error object</param>
        internal static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{error.Message} \n[line {error.Token.Line}]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Reports errors to stdout
        /// </summary>
        /// <param name="line">Line of original source text error occured</param>
        /// <param name="where">Location error occured</param>
        /// <param name="message">Error message</param>
        private static void Report(int line, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Run Lox code
        /// </summary>
        /// <param name="source">Text of Lox code</param>
        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (_hadError) return;

            Resolver resolver = new Resolver(Interpreter);
            resolver.Resolve(statements);

            Interpreter.Interpret(statements);
        }

        /// <summary>
        /// Runs Lox file
        /// </summary>
        /// <param name="path">Path to file</param>
        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));

            if (_hadError) System.Environment.Exit(65);
            if (_hadRuntimeError) System.Environment.Exit(70);
        }

        /// <summary>
        /// Runs Lox in interactive mode
        /// </summary>
        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write(">> ");
                Run(Console.ReadLine());
                _hadError = false;
            }
        }
    }
}
