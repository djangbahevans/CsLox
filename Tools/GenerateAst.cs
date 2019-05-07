﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Tools
{
    class GenerateAst
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output_directory>");
                Environment.Exit(1);
            }
            String outputDir = args[0];
            defineAst(outputDir, "Expr", new List<string>(new string[]
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token op, Expr right",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Unary    : Token op, Expr right",
                "Variable : Token name"
            }));
            defineAst(outputDir, "Stmt", new List<string>(new string[]
            {
                "Block      : List<Stmt> statements",
                "Expression : Expr expression",
                "Print      : Expr expression",
                "Var        : Token name, Expr initializer"
            }));
        }

        private static void defineAst(string outputDir, string baseName, List<string> types)
        {
            string path = $"{outputDir}{baseName}.cs";
            //Stream stream = File.Create(path);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("// This is an autogenerated file. Do not change.");
                writer.WriteLine();
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine("namespace CsLox");
                writer.WriteLine("{");
                writer.WriteLine($"    abstract class {baseName}");
                writer.WriteLine("    {");

                DefineVisitor(writer, baseName, types);
                writer.WriteLine();

                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(writer, baseName, className, fields);

                    writer.WriteLine("        }");
                    writer.WriteLine();
                }
                writer.WriteLine("        public abstract T Accept<T>(IVisitor<T> visitor);");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine($"        public class {className} : { baseName}");
            writer.WriteLine("        {");

            // Constructor
            writer.WriteLine($"            public {className}({fieldList})");
            writer.WriteLine("            {");

            // Store parameters in fields
            String[] fields = fieldList.Split(", ");
            foreach (String field in fields)
            {
                String name = field.Split(" ")[1];
                writer.WriteLine($"                this.{ name} = { name};");
            }

            writer.WriteLine("            }");

            // Visitor pattern
            writer.WriteLine();
            writer.WriteLine("            public override T Accept<T>(IVisitor<T> visitor)");
            writer.WriteLine("            {");
            writer.WriteLine($"                return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine("            }");

            // Fields
            writer.WriteLine();
            foreach (String field in fields)
            {
                writer.WriteLine($"            public readonly {field};");
            }

            //writer.WriteLine("  }");
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("        public interface IVisitor<T>");
            writer.WriteLine("        {");
            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.WriteLine($"            T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            writer.WriteLine("        }");
        }
    }
}
