using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection
{
    public class ExceptionRaisedEventArgs : EventArgs
    {
        public string MethodName { get; }
        public Exception Exception { get; }

        public ExceptionRaisedEventArgs(string methodName, Exception exception)
        {
            Guard.AgainstNullOrEmptyString(methodName, nameof(methodName));
            Guard.AgainstNull(exception, nameof(exception));

            MethodName = methodName;
            Exception = exception;
        }
    }
}