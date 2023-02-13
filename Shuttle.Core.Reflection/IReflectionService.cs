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
        Task<Assembly> GetAssembly(string assemblyPath);
        Task<Assembly> FindAssemblyNamed(string name);
        Task<IEnumerable<Assembly>> GetMatchingAssemblies(Regex regex);
        Task<IEnumerable<Type>> GetTypesAssignableTo(Type type);
        Task<IEnumerable<Type>> GetTypes(Assembly assembly);
        Task<IEnumerable<Type>> GetTypesAssignableTo(Type type, Assembly assembly);
        Task<Type> GetType(string typeName);
        Task<IEnumerable<Assembly>> GetRuntimeAssemblies();
    }
}
