using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    class Lox
    {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: CsLox [script]");
                return 64;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
            return 0;
        }

        internal static void RuntimeError(RuntimeError error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{error.Message} \n[line {error.Token.Line}]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write(">> ");
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            if (hadError) return;

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        internal static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF) Report(token.Line, "at end", message);
            else Report(token.Line, $" at '{token.Lexeme}'", message);
        }
    }
}
