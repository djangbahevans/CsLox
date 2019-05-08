using System;

namespace CsLox
{
    [Serializable]
    internal class RuntimeError : SystemException
    {
        public RuntimeError(Token token, string message) : base(message)
        {
            this.Token = token;
        }

        internal Token Token { get; }
    }
}