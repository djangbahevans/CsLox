using System.Collections.Generic;

namespace CsLox
{
    internal class Environment
    {
        public Environment Enclosing { get; }
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance)._values.Add(name.Lexeme, value);
        }

        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (_values.ContainsKey(name.Lexeme)) return _values[name.Lexeme];
            if (Enclosing != null) return Enclosing.Get(name);
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance)._values[name];
        }

        internal void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (Enclosing == null) throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
            Enclosing.Assign(name, value);
            return;
        }
        private Environment Ancestor(int distance)
        {
            Environment environment = this;
            for (int i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment;
        }
    }
}