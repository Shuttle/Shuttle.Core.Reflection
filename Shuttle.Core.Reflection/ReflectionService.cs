using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shuttle.Core.Contract;
#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace Shuttle.Core.Reflection
{
    public class ReflectionService : IReflectionService
    {
        private readonly List<string> _assemblyExtensions = new List<string>
        {
            ".dll",
            ".exe"
        };

        private readonly ILogger<ReflectionService> _logger;

        public ReflectionService() : this(null)
        {
        }

        public ReflectionService(ILogger<ReflectionService> logger)
        {
            _logger = logger ?? new NullLogger<ReflectionService>();
        }

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
                var fileName = Path.GetFileNameWithoutExtension(assemblyPath);

#if NETCOREAPP
                var inCompileLibraries = DependencyContext.Default.CompileLibraries.Any(library => library.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
                var inRuntimeLibraries = DependencyContext.Default.RuntimeLibraries.Any(library => library.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                var assembly = (inCompileLibraries || inRuntimeLibraries)
                    ? Assembly.Load(new AssemblyName(fileName))
                    : AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

                return assembly;
#else
                return Assembly.Load(new AssemblyName(fileName));
#endif

            }
            catch (Exception ex)
            {
                _logger.LogWarning(string.Format(Resources.AssemblyLoadException, assemblyPath, ex.Message));

                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    if (ex is ReflectionTypeLoadException reflection)
                    {
                        foreach (var exception in reflection.LoaderExceptions)
                        {
                            _logger.LogTrace($"'{exception.Message}'.");
                        }
                    }
                    else
                    {
                        _logger.LogTrace($"{ex.GetType()}: '{ex.Message}'.");
                    }
                }

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

        public IEnumerable<Type> GetTypesAssignableTo(Type type, Assembly assembly)
        {
            Guard.AgainstNull(type, nameof(type));
            Guard.AgainstNull(assembly, nameof(assembly));

            return GetTypes(assembly).Where(candidate =>
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

        public IEnumerable<Type> GetTypes(Assembly assembly)
        {
            Type[] types;

            try
            {
                _logger.LogTrace(string.Format(Resources.TraceGetTypesFromAssembly, assembly));

                types = assembly.GetTypes();
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException reflection)
                {
                    foreach (var exception in reflection.LoaderExceptions)
                    {
                        _logger.LogError($"'{exception.Message}'.");
                    }
                }
                else
                {
                    _logger.LogError($"{ex.GetType()}: '{ex.Message}'.");
                }

                return new List<Type>();
            }

            return types;
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
    }
}