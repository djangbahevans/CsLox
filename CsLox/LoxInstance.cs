using System.Collections.Generic;

namespace CsLox
{
    internal class LoxInstance
    {
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();
        private LoxClass _class;

        public LoxInstance(LoxClass @class)
        {
            _class = @class;
        }

        public object Get(Token name)
        {
            if (fields.ContainsKey(name.Lexeme)) return fields[name.Lexeme];

            LoxFunction method = _class.FindMethod(name.Lexeme);
            if (method != null) return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            fields.Add(name.Lexeme, value);
        }

        public override string ToString()
        {
            return _class.Name;
        }
    }
}
