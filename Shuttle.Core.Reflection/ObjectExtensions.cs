using System;

namespace Shuttle.Core.Reflection
{
    public static class ObjectExtensions
    {
        public static void AttemptDispose(this object o)
        {
            if (o is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}