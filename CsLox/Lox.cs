using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    class Lox
    {
        static bool hadError = false;
        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                System.Console.WriteLine("Usage: CsLox [script]");
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
            string file = File.ReadAllText(path);
            Run(file);

            if (hadError)
            {
                Environment.Exit(65);
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();
            Parser parser = new Parser(tokens);
            Expr expression = parser.Parse();

            if (hadError) return;

            Console.WriteLine(new AstPrinter().Print(expression));
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine("[line " + line + "] Error" + where + ": " + message);
        }

        internal static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF) Report(token.line, " at end", message);
            else Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }
}
