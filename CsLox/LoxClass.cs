using System.Collections.Generic;

namespace CsLox
{
    internal class LoxClass : ILoxCallable
    {
        private readonly IDictionary<string, LoxFunction> _methods;
        public LoxClass(string name, LoxClass superclass, IDictionary<string, LoxFunction> methods)
        {

            Name = name;
            Superclass = superclass;
            _methods = methods;
        }

        public LoxClass Superclass { get; set; }
        internal string Name { get; }
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

            if (Superclass != null) return Superclass.FindMethod(name);

            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
