using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection;

public class ReflectionService : IReflectionService
{
    public async Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex regex)
    {
        Guard.AgainstNull(regex);

        return (await GetRuntimeAssembliesAsync().ConfigureAwait(false)).Where(assembly => regex.IsMatch(assembly.FullName ?? string.Empty)).ToList();
    }

    public async Task<IEnumerable<Assembly>> GetRuntimeAssembliesAsync()
    {
        var result = new List<Assembly>();
        var dependencyContext = DependencyContext.Default;

        if (dependencyContext != null)
        {
            foreach (var runtimeAssemblyName in dependencyContext.GetRuntimeAssemblyNames(Environment.OSVersion.Platform.ToString()))
            {
                result.Add(Assembly.Load(runtimeAssemblyName));
            }
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (result.All(item => item.GetName().Equals(assembly.GetName())))
            {
                result.Add(assembly);
            }
        }

        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type)
    {
        var result = new List<Type>();

        var assemblies = await this.GetAssembliesAsync().ConfigureAwait(false);

        foreach (var assembly in assemblies)
        {
            var types = await GetTypesAssignableToAsync(type, assembly).ConfigureAwait(false);

            types.Where(candidate => result.Find(existing => existing == candidate) == null)
                .ToList()
                .ForEach(add => result.Add(add));
        }

        return result;
    }

    public async Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type, Assembly assembly)
    {
        Guard.AgainstNull(type);

        return await Task.FromResult(Guard.AgainstNull(assembly).GetTypes().Where(item => TypeExtensions.IsAssignableTo(item, type) && !(item.IsInterface && item == type)).ToList());
    }

    public async Task<Type?> GetTypeAsync(string typeName)
    {
        return await Task.FromResult(Type.GetType(Guard.AgainstNullOrEmptyString(typeName),
            assemblyName =>
            {
                Assembly assembly;

                try
                {
                    assembly = Assembly.LoadFrom(
                        Path.Combine(string.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath)
                                ? AppDomain.CurrentDomain.BaseDirectory
                                : AppDomain.CurrentDomain.RelativeSearchPath, $"{assemblyName.Name}.dll"));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format(Resources.AssemblyLoadException, assemblyName.Name, typeName, ex.Message));
                }

                return assembly;
            },
            (assembly, typeNameSought, ignore) => assembly == null
                ? Type.GetType(typeNameSought, false, ignore)
                : assembly.GetType(typeNameSought, false, ignore)));
    }
}