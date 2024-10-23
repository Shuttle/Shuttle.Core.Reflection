using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection;

public static class EnumerableExtensions
{
    public static T? Find<T>(this IEnumerable<object> list) where T : class
    {
        var matches = FindAll<T>(list).ToList();

        if (matches.Count > 1)
        {
            throw new InvalidOperationException(string.Format(Resources.EnumerableFoundTooManyException, matches.Count, typeof(T).FullName));
        }

        return matches.Count == 1
            ? matches[0]
            : null;
    }

    public static IEnumerable<T> FindAll<T>(this IEnumerable<object> list) where T : class
    {
        var type = typeof(T);

        return Guard.AgainstNull(list).Where(o => TypeExtensions.IsAssignableTo(o.GetType(), type)).Select(o => (T)o).ToList();
    }

    public static T Get<T>(this IEnumerable<object> list) where T : class
    {
        var result = Find<T>(list);

        if (result == null)
        {
            throw new InvalidOperationException(string.Format(Resources.EnemerableNoMatchException, typeof(T).FullName));
        }

        return result;
    }
}