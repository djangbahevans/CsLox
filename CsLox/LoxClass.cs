using System.Collections.Generic;

namespace CsLox
{
    internal class LoxClass : ILoxCallable
    {
        internal string Name { get; }
        private readonly IDictionary<string, LoxFunction> _methods;

        public LoxClass(string name, IDictionary<string, LoxFunction> methods)
        {
            this.Name = name;
            _methods = methods;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Arity()
        {
            LoxFunction initializer = FindMethod("init");
            return initializer?.Arity() ?? 0;
            ;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initializer = FindMethod("init");
            initializer?.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        public LoxFunction FindMethod(string name)
        {
            if (_methods.ContainsKey(name)) return _methods[name];

            return null;
        }
    }
}
