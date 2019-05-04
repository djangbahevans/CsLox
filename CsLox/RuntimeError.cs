using System;
using System.Runtime.Serialization;

namespace CsLox
{
    [Serializable]
    internal class RuntimeError : SystemException
    {
        private readonly Token token;

        public RuntimeError(Token token, string message) : base(message)
        {
            this.token = token;
        }

        internal Token Token => token;
    }
}