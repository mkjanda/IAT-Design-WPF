using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Exceptions
{
    internal class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
