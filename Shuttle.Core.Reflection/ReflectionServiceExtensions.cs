using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;

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
    }
}