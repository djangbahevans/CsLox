using System.Collections.Generic;

namespace CsLox
{
    internal interface ILoxCallable
    {
        int Arity();

        object Call(Interpreter interpreter, List<object> arguments);
    }
}
