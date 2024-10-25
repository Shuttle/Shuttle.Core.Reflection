using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shuttle.Core.Reflection;

public interface IReflectionService
{
    Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex regex);
    Task<IEnumerable<Assembly>> GetRuntimeAssembliesAsync();
    Task<Type?> GetTypeAsync(string typeName);
    Task<IEnumerable<Type>> GetTypesAssignableToExpandedAsync(Type type);
    Task<IEnumerable<Type>> GetTypesAssignableToExpandedAsync(Type type, Assembly assembly);
}