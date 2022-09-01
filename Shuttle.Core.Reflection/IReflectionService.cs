using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

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
    }
}
