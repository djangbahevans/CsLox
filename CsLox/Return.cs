using System;

namespace CsLox
{
    internal class Return : SystemException
    {
        internal object Value { get; }

        public Return(object value)
        {
            Value = value;
        }
    }
}
