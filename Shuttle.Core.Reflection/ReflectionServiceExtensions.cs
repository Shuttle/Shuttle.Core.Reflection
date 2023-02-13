using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection
{
    public static class ReflectionServiceExtensions
    {
        public static async Task<IEnumerable<Type>> GetTypesAssignableTo<T>(this IReflectionService service)
        {
            Guard.AgainstNull(service, nameof(service));
            
            return await service.GetTypesAssignableTo(typeof(T)).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Type>> GetTypesAssignableTo<T>(this IReflectionService service, Assembly assembly)
        {
            Guard.AgainstNull(service, nameof(service));

            return await service.GetTypesAssignableTo(typeof(T), assembly).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Assembly>> GetAssemblies(this IReflectionService service)
        {
            return await service.GetMatchingAssemblies(new Regex(".*")).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Assembly>> GetMatchingAssemblies(this IReflectionService service, string regex)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNullOrEmptyString(regex, nameof(regex));

            return await service.GetMatchingAssemblies(new Regex(regex)).ConfigureAwait(false);
        }
    }
}