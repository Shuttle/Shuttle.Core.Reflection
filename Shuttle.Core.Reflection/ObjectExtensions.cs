using System;

namespace Shuttle.Core.Reflection
{
    public static class ObjectExtensions
    {
        public static void TryDispose(this object o)
        {
            if (o is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}