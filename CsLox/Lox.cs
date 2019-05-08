using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    public class Lox
    {
        private static readonly Interpreter Interpreter = new Interpreter();
        private static bool _hadError;
        private static bool _hadRuntimeError = false;

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

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

        internal static void Error(Token token, string message) =>
            Report(token.Line, token.Type == TokenType.EOF ? "at end" : $" at '{token.Lexeme}'", message);

        internal static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{error.Message} \n[line {error.Token.Line}]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void Report(int line, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (_hadError) return;

            Interpreter.Interpret(statements);
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));

            if (_hadError) System.Environment.Exit(65);
            if (_hadRuntimeError) System.Environment.Exit(70);
        }

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
