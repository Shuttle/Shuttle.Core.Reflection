using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection;

public static class TypeExtensions
{
    public static void AssertDefaultConstructor(this Type type)
    {
        Guard.AgainstNull(type);

        AssertDefaultConstructor(type, $"Type '{type.FullName}' does not have a default constructor.");
    }

    public static void AssertDefaultConstructor(this Type type, string message)
    {
        if (!type.HasDefaultConstructor())
        {
            throw new NotSupportedException(message);
        }
    }

    public static Type? FirstInterface(this Type type, Type of)
    {
        var interfaces = type.GetInterfaces();
        var name = $"I{type.Name}";

        foreach (var i in interfaces)
        {
            if (i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return i;
            }
        }

        return interfaces.FirstOrDefault(item => IsAssignableToExpanded(item, of));
    }

    public static Type? GetGenericArgument(this Type type, Type generic)
    {
        return type.GetInterfaces()
            .Where(item => item.IsGenericType && item.GetGenericTypeDefinition().IsAssignableFrom(generic))
            .Select(item => item.GetGenericArguments()[0]).FirstOrDefault();
    }

    public static bool HasDefaultConstructor(this Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null;
    }

    public static Type? InterfaceMatching(this Type type, string includeRegexPattern, string? excludeRegexPattern = null)
    {
        var includeRegex = new Regex(includeRegexPattern, RegexOptions.IgnoreCase);
        
        Regex? excludeRegex = null;

        if (!string.IsNullOrEmpty(excludeRegexPattern))
        {
            excludeRegex = new(excludeRegexPattern, RegexOptions.IgnoreCase);
        }

        foreach (var i in type.GetInterfaces())
        {
            var fullName = i.FullName ?? string.Empty;

            if (includeRegex.IsMatch(fullName) && (excludeRegex == null || !excludeRegex.IsMatch(fullName)))
            {
                return i;
            }
        }

        return null;
    }

    public static IEnumerable<Type> InterfacesAssignableToExpanded<T>(this Type type)
    {
        return type.InterfacesAssignableToExpanded(typeof(T));
    }

    public static IEnumerable<Type> InterfacesAssignableToExpanded(this Type type, Type interfaceType)
    {
        Guard.AgainstNull(interfaceType);

        return type.GetInterfaces().Where(i => IsAssignableToExpanded(i, interfaceType)).ToList();
    }

    public static bool IsAssignableToExpanded(this Type type, Type otherType)
    {
        Guard.AgainstNull(type);
        Guard.AgainstNull(otherType);

        return type.IsGenericType && otherType.IsGenericType
            ? otherType.GetGenericTypeDefinition().IsAssignableFrom(type.GetGenericTypeDefinition())
            : otherType.IsGenericType
                ? IsAssignableToGenericType(type, otherType)
                : otherType.IsAssignableFrom(type);
    }

    private static bool IsAssignableToGenericType(Type type, Type generic)
    {
        return
            type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition().IsAssignableFrom(generic));
    }

    /// <summary>
    ///     Returns a IEnumerable&lt;Type&gt; containing the interface that has the same name as the type prefixed by an 'I';
    ///     else null.
    /// </summary>
    /// <param name="type"></param>
    /// <example>If the type name is Example it will try to locate interface IExample.</example>
    /// <returns></returns>
    public static Type? MatchingInterface(this Type type)
    {
        var name = $"I{type.Name}";

        return type.GetInterfaces()
            .Where(i => i.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            .Select(i => i)
            .FirstOrDefault();
    }
}