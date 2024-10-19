using System;
using System.Threading.Tasks;

namespace Shuttle.Core.Reflection;

public static class ObjectExtensions
{
    public static void TryDispose(this object o)
    {
        if (o is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public static async Task TryDisposeAsync(this object o)
    {
        if (o is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            o.TryDispose();
        }
    }
}