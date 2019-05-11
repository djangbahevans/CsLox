using System;

namespace CsLox
{
    /// <summary>
    /// Handles function return cases and sets a return value if any.
    /// </summary>
    internal class Return : SystemException
    {
        /// <summary>
        /// Return value
        /// </summary>
        internal object Value { get; }

        public Return(object value)
        {
            Value = value;
        }
    }
}
