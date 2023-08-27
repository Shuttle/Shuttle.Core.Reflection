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
        public static IEnumerable<Type> GetTypesAssignableTo<T>(this IReflectionService service)
        {
            Guard.AgainstNull(service, nameof(service));

            return service.GetTypesAssignableTo(typeof(T));
        }

        public static IEnumerable<Type> GetTypesAssignableTo<T>(this IReflectionService service, Assembly assembly)
        {
            Guard.AgainstNull(service, nameof(service));

            return service.GetTypesAssignableTo(typeof(T), assembly);
        }

        public static IEnumerable<Assembly> GetAssemblies(this IReflectionService service)
        {
            return service.GetMatchingAssemblies(new Regex(".*"));
        }

        public static IEnumerable<Assembly> GetMatchingAssemblies(this IReflectionService service, string regex)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNullOrEmptyString(regex, nameof(regex));

            return service.GetMatchingAssemblies(new Regex(regex));
        }

        public static async Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>(this IReflectionService service)
        {
            Guard.AgainstNull(service, nameof(service));
            
            return await service.GetTypesAssignableToAsync(typeof(T)).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>(this IReflectionService service, Assembly assembly)
        {
            Guard.AgainstNull(service, nameof(service));

            return await service.GetTypesAssignableToAsync(typeof(T), assembly).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Assembly>> GetAssembliesAsync(this IReflectionService service)
        {
            return await service.GetMatchingAssembliesAsync(new Regex(".*")).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(this IReflectionService service, string regex)
        {
            Guard.AgainstNull(service, nameof(service));
            Guard.AgainstNullOrEmptyString(regex, nameof(regex));

            return await service.GetMatchingAssembliesAsync(new Regex(regex)).ConfigureAwait(false);
        }
    }
}