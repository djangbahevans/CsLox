using System.Collections.Generic;

namespace CsLox
{
    internal class Environment
    {
        private readonly Environment _enclosing;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        public Environment()
        {
            _enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this._enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (_values.ContainsKey(name.Lexeme)) return _values[name.Lexeme];
            if (_enclosing != null) return _enclosing.Get(name);
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'");
        }

        internal void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (_enclosing == null) throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
            _enclosing.Assign(name, value);
            return;
        }
    }
}