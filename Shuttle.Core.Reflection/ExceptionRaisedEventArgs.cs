using System;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection;

public class ExceptionRaisedEventArgs : EventArgs
{
    public ExceptionRaisedEventArgs(string methodName, Exception exception)
    {
        MethodName = Guard.AgainstNullOrEmptyString(methodName);
        Exception = Guard.AgainstNull(exception);
    }

    public Exception Exception { get; }
    public string MethodName { get; }
}