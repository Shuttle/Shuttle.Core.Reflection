using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Reflection
{
    public class ReflectionService : IReflectionService
    {
        private readonly List<string> _assemblyExtensions = new List<string>
        {
            ".dll",
            ".exe"
        };

        public event EventHandler<ExceptionRaisedEventArgs> ExceptionRaised;

        public string AssemblyPath(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            return !assembly.IsDynamic
                ? new Uri(Uri.UnescapeDataString(new UriBuilder(assembly.CodeBase).Path)).LocalPath
                : string.Empty;
        }

        public Assembly GetAssembly(string assemblyPath)
        {
            var result = GetRuntimeAssemblies()
                .FirstOrDefault(assembly => AssemblyPath(assembly)
                    .Equals(assemblyPath, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            try
            {
                return Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath)));
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionRaisedEventArgs($"GetAssembly({assemblyPath})", ex));

                return null;
            }
        }

        public async Task<Assembly> GetAssemblyAsync(string assemblyPath)
        {
            var assemblies = await GetRuntimeAssembliesAsync().ConfigureAwait(false);

            var result = assemblies.FirstOrDefault(assembly => AssemblyPath(assembly).Equals(assemblyPath, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            try
            {
                return Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(assemblyPath)));
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionRaisedEventArgs($"GetAssembly({assemblyPath})", ex));

                return null;
            }
        }

        public Assembly FindAssemblyNamed(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var assemblyName = name;
            var hasFileExtension = false;

            if (name.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)
                ||
                name.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                assemblyName = Path.GetFileNameWithoutExtension(name);
                hasFileExtension = true;
            }

            var result = GetRuntimeAssemblies()
                .FirstOrDefault(assembly => assembly.GetName()
                    .Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            var privateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);

            var extensions = new List<string>();

            if (hasFileExtension)
            {
                extensions.Add(string.Empty);
            }
            else
            {
                extensions.AddRange(_assemblyExtensions);
            }

            foreach (var extension in extensions)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(name, extension));

                if (File.Exists(path))
                {
                    return GetAssembly(path);
                }

                if (!privateBinPath.Equals(AppDomain.CurrentDomain.BaseDirectory))
                {
                    path = Path.Combine(privateBinPath, string.Concat(name, extension));

                    if (File.Exists(path))
                    {
                        return GetAssembly(path);
                    }
                }
            }

            return null;
        }

        public async Task<Assembly> FindAssemblyNamedAsync(string name)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            var assemblyName = name;
            var hasFileExtension = false;

            if (name.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase)
                ||
                name.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
            {
                assemblyName = Path.GetFileNameWithoutExtension(name);
                hasFileExtension = true;
            }

            var assemblies = await GetRuntimeAssembliesAsync().ConfigureAwait(false);

            var result = assemblies.FirstOrDefault(assembly => assembly.GetName()
                .Name.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase));

            if (result != null)
            {
                return result;
            }

            var privateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);

            var extensions = new List<string>();

            if (hasFileExtension)
            {
                extensions.Add(string.Empty);
            }
            else
            {
                extensions.AddRange(_assemblyExtensions);
            }

            foreach (var extension in extensions)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Concat(name, extension));

                if (File.Exists(path))
                {
                    return await GetAssemblyAsync(path);
                }

                if (!privateBinPath.Equals(AppDomain.CurrentDomain.BaseDirectory))
                {
                    path = Path.Combine(privateBinPath, string.Concat(name, extension));

                    if (File.Exists(path))
                    {
                        return await GetAssemblyAsync(path);
                    }
                }
            }

            return null;
        }

        public async Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex regex)
        {
            Guard.AgainstNull(regex, nameof(regex));

            var result = (await GetRuntimeAssembliesAsync().ConfigureAwait(false)).Where(assembly => regex.IsMatch(assembly.FullName)).ToList();

            foreach (
                var assembly in
                (await GetMatchingAssembliesAsync(regex, AppDomain.CurrentDomain.BaseDirectory).ConfigureAwait(false))
                .Where(assembly => result.Find(candidate => candidate.Equals(assembly)) == null))
            {
                result.Add(assembly);
            }

            var privateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);

            if (!privateBinPath.Equals(AppDomain.CurrentDomain.BaseDirectory))
            {
                foreach (
                    var assembly in
                    (await GetMatchingAssembliesAsync(regex, privateBinPath).ConfigureAwait(false))
                    .Where(assembly => result.Find(candidate => candidate.Equals(assembly)) == null))
                {
                    result.Add(assembly);
                }
            }

            return result;
        }

        public IEnumerable<Assembly> GetRuntimeAssemblies()
        {
            var result = new List<Assembly>();
            var dependencyContext = DependencyContext.Default;

            if (dependencyContext != null)
            {
                foreach (var runtimeAssemblyName in dependencyContext.GetRuntimeAssemblyNames(Environment.OSVersion
                             .Platform.ToString()))
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

            return result;
        }

        public async Task<IEnumerable<Assembly>> GetRuntimeAssembliesAsync()
        {
            var result = new List<Assembly>();
            var dependencyContext = DependencyContext.Default;

            if (dependencyContext != null)
            {
                foreach (var runtimeAssemblyName in dependencyContext.GetRuntimeAssemblyNames(Environment.OSVersion
                             .Platform.ToString()))
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

        public IEnumerable<Type> GetTypesAssignableTo(Type type)
        {
            var result = new List<Type>();

            foreach (var assembly in this.GetAssemblies())
            {
                GetTypesAssignableTo(type, assembly)
                    .Where(candidate => result.Find(existing => existing == candidate) == null)
                    .ToList()
                    .ForEach(add => result.Add(add));
            }

            return result;
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

        public IEnumerable<Type> GetTypesAssignableTo(Type type, Assembly assembly)
        {
            Guard.AgainstNull(type, nameof(type));
            Guard.AgainstNull(assembly, nameof(assembly));

            return GetTypes(assembly).Where(candidate =>
                candidate.IsAssignableTo(type) && !(candidate.IsInterface && candidate == type)).ToList();
        }

        public async Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type, Assembly assembly)
        {
            Guard.AgainstNull(type, nameof(type));
            Guard.AgainstNull(assembly, nameof(assembly));

            var types = await GetTypesAsync(assembly).ConfigureAwait(false);

            return types.Where(candidate =>
                candidate.IsAssignableTo(type) && !(candidate.IsInterface && candidate == type)).ToList();
        }

        public Type GetType(string typeName)
        {
            return Type.GetType(typeName,
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
                        throw new ApplicationException(string.Format(Resources.AssemblyLoadException, assemblyName.Name,
                            typeName, ex.Message));
                    }

                    return assembly;
                },
                (assembly, typeNameSought, ignore) => assembly == null
                    ? Type.GetType(typeNameSought, false, ignore)
                    : assembly.GetType(typeNameSought, false, ignore));
        }

        public async Task<Type> GetTypeAsync(string typeName)
        {
            return await Task.FromResult(GetType(typeName));
        }

        public IEnumerable<Type> GetTypes(Assembly assembly)
        {
            Guard.AgainstNull(assembly, nameof(assembly));

            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (Exception ex)
            {
                ExceptionRaised?.Invoke(this, new ExceptionRaisedEventArgs($"GetTypes({assembly.FullName})", ex));

                return new List<Type>();
            }

            return types;
        }

        public async Task<IEnumerable<Type>> GetTypesAsync(Assembly assembly)
        {
            return await Task.FromResult(GetTypes(assembly));
        }

        public IEnumerable<Assembly> GetMatchingAssemblies(Regex regex)
        {
            Guard.AgainstNull(regex, nameof(regex));

            var assemblies =
                new List<Assembly>(GetRuntimeAssemblies().Where(assembly => regex.IsMatch(assembly.FullName)));

            foreach (
                var assembly in
                GetMatchingAssemblies(regex, AppDomain.CurrentDomain.BaseDirectory)
                    .Where(assembly => assemblies.Find(candidate => candidate.Equals(assembly)) == null))
            {
                assemblies.Add(assembly);
            }

            var privateBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.RelativeSearchPath ?? string.Empty);

            if (!privateBinPath.Equals(AppDomain.CurrentDomain.BaseDirectory))
            {
                foreach (
                    var assembly in
                    GetMatchingAssemblies(regex, privateBinPath)
                        .Where(assembly => assemblies.Find(candidate => candidate.Equals(assembly)) == null))
                {
                    assemblies.Add(assembly);
                }
            }

            return assemblies;
        }

        private IEnumerable<Assembly> GetMatchingAssemblies(Regex expression, string folder)
        {
            var result = new List<Assembly>();

            if (Directory.Exists(folder))
            {
                result.AddRange(
                    Directory.GetFiles(folder, "*.exe")
                        .Where(file => expression.IsMatch(Path.GetFileNameWithoutExtension(file)))
                        .Select(GetAssembly)
                        .Where(assembly => assembly != null));
                result.AddRange(
                    Directory.GetFiles(folder, "*.dll")
                        .Where(file => expression.IsMatch(Path.GetFileNameWithoutExtension(file)))
                        .Select(GetAssembly)
                        .Where(assembly => assembly != null));
            }

            return result;
        }

        private async Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex expression, string folder)
        {
            var result = new List<Assembly>();

            if (Directory.Exists(folder))
            {
                result.AddRange(await Task.WhenAll(
                    Directory.GetFiles(folder, "*.exe")
                        .Where(file => expression.IsMatch(Path.GetFileNameWithoutExtension(file)))
                        .Select(GetAssemblyAsync)
                        .Where(assembly => assembly != null)));
                result.AddRange(await Task.WhenAll(
                    Directory.GetFiles(folder, "*.dll")
                        .Where(file => expression.IsMatch(Path.GetFileNameWithoutExtension(file)))
                        .Select(async assemblyPath => await GetAssemblyAsync(assemblyPath).ConfigureAwait(false))
                        .Where(assembly => assembly != null)));
            }

            return result;
        }
    }
}