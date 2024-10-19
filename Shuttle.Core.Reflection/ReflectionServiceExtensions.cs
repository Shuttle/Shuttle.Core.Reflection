using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection;

public static class ReflectionServiceExtensions
{
    public static async Task<IEnumerable<Assembly>> GetAssembliesAsync(this IReflectionService service)
    {
        return await service.GetMatchingAssembliesAsync(new Regex(".*")).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(this IReflectionService service, string regex)
    {

        return await Guard.AgainstNull(service).GetMatchingAssembliesAsync(new(Guard.AgainstNullOrEmptyString(regex))).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>(this IReflectionService service)
    {
        return await Guard.AgainstNull(service).GetTypesAssignableToAsync(typeof(T)).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>(this IReflectionService service, Assembly assembly)
    {
        return await Guard.AgainstNull(service).GetTypesAssignableToAsync(typeof(T), assembly).ConfigureAwait(false);
    }
}