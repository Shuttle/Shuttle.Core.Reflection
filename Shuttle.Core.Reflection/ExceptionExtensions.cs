using System;
using System.Text;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection
{
    public static class ExceptionExtensions
    {
        public static T Find<T>(this Exception ex) where T : Exception
        {
            var enumerator = ex;

            while (enumerator != null)
            {
                if (enumerator is T result)
                {
                    return result;
                }

                enumerator = enumerator.InnerException;
            }

            return null;
        }

        public static bool Contains<T>(this Exception ex) where T : Exception
        {
            return ex.Find<T>() != null;
        }

        public static string AllMessages(this Exception ex)
        {
            var messages = new StringBuilder();

            var enumerator = ex;

            while (enumerator != null)
            {
                messages.AppendFormat("{0}{1}", messages.Length > 0 ? " / " : string.Empty, enumerator.Message);

                enumerator = enumerator.InnerException;
            }

            return messages.ToString();
        }

        public static Exception TrimLeading<T>(this Exception ex) where T : Exception
        {
            Guard.AgainstNull(ex, nameof(ex));

            var trim = typeof(T);

            var exception = ex;

            while (exception.GetType() == trim)
            {
                if (exception.InnerException == null)
                {
                    break;
                }

                exception = exception.InnerException;
            }

            return exception;
        }
    }
}