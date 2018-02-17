using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shuttle.Core.Reflection
{
    public interface IReflectionService
    {
	    string AssemblyPath(Assembly assembly);
		Assembly GetAssembly(string assemblyPath);
		Assembly FindAssemblyNamed(string name);
		IEnumerable<Assembly> GetAssemblies(string folder);
		IEnumerable<Assembly> GetAssemblies();
        IEnumerable<Assembly> GetMatchingAssemblies(string regex, string folder);
		IEnumerable<Assembly> GetMatchingAssemblies(string regex);
		IEnumerable<Assembly> GetRuntimeAssemblies();
		IEnumerable<Type> GetTypesAssignableTo<T>();
		IEnumerable<Type> GetTypesAssignableTo(Type type);
		IEnumerable<Type> GetTypes(Assembly assembly);
		IEnumerable<Type> GetTypesAssignableTo<T>(Assembly assembly);
		IEnumerable<Type> GetTypesAssignableTo(Type type, Assembly assembly);
    }
}
