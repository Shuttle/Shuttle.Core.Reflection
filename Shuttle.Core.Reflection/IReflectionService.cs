using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shuttle.Core.Reflection
{
    public interface IReflectionService
    {
        event EventHandler<ExceptionRaisedEventArgs> ExceptionRaised;

        string AssemblyPath(Assembly assembly);

        Assembly GetAssembly(string assemblyPath);
        Assembly FindAssemblyNamed(string name);
        IEnumerable<Assembly> GetMatchingAssemblies(Regex regex);
        IEnumerable<Type> GetTypesAssignableTo(Type type);
        IEnumerable<Type> GetTypes(Assembly assembly);
        IEnumerable<Type> GetTypesAssignableTo(Type type, Assembly assembly);
        Type GetType(string typeName);
        IEnumerable<Assembly> GetRuntimeAssemblies();

        Task<Assembly> GetAssemblyAsync(string assemblyPath);
        Task<Assembly> FindAssemblyNamedAsync(string name);
        Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex regex);
        Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type);
        Task<IEnumerable<Type>> GetTypesAsync(Assembly assembly);
        Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type, Assembly assembly);
        Task<Type> GetTypeAsync(string typeName);
        Task<IEnumerable<Assembly>> GetRuntimeAssembliesAsync();
    }
}
