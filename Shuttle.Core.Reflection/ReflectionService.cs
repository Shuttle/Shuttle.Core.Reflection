using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Shuttle.Core.Contract;
using Shuttle.Core.Logging;
#if (NETCOREAPP2_0 || NETCOREAPP2_1 || NETSTANDARD2_0)
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyModel;
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

        private readonly ILog _log;

        public ReflectionService()
        {
            _log = Log.For(this);
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
                switch (Path.GetExtension(assemblyPath))
                {
                    case ".dll":
                    case ".exe":
                        {
                            result = Path.GetDirectoryName(assemblyPath) == AppDomain.CurrentDomain.BaseDirectory
                                ? Assembly.Load(Path.GetFileNameWithoutExtension(assemblyPath) ??
                                                throw new InvalidOperationException(string.Format(Resources.GetFileNameWithoutExtensionException, assemblyPath)))
                                : Assembly.LoadFile(assemblyPath);
                            break;
                        }

                    default:
                        {
                            result = Assembly.Load(assemblyPath);

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _log.Warning(string.Format(Resources.AssemblyLoadException, assemblyPath, ex.Message));

                if (ex is ReflectionTypeLoadException reflection)
                {
                    foreach (var exception in reflection.LoaderExceptions)
                    {
                        _log.Trace($"'{exception.Message}'.");
                    }
                }
                else
                {
                    _log.Trace($"{ex.GetType()}: '{ex.Message}'.");
                }

                return null;
            }

            return result;
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

        public IEnumerable<Assembly> GetAssemblies(string folder)
        {
            return GetMatchingAssemblies(string.Empty, folder);
        }

        public IEnumerable<Assembly> GetAssemblies()
        {
            return GetMatchingAssemblies(string.Empty);
        }

        public IEnumerable<Assembly> GetMatchingAssemblies(string regex, string folder)
        {
            return GetMatchingAssemblies(new Regex(regex, RegexOptions.IgnoreCase), folder);
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

        public IEnumerable<Assembly> GetMatchingAssemblies(string regex)
        {
            var assemblies = new List<Assembly>(GetRuntimeAssemblies());

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
#if (!NETCOREAPP2_0 && !NETCOREAPP2_1 && !NETSTANDARD2_0)
            return AppDomain.CurrentDomain.GetAssemblies();
#else
            var result = new List<Assembly>();

	        foreach (var runtimeAssemblyName in DependencyContext.Default.GetRuntimeAssemblyNames(RuntimeEnvironment.GetRuntimeIdentifier()))
	        {
	            result.Add(Assembly.Load(runtimeAssemblyName));
	        }

	        return result;
#endif
        }

        public IEnumerable<Type> GetTypesAssignableTo<T>()
        {
            return GetTypesAssignableTo(typeof(T));
        }

        public IEnumerable<Type> GetTypesAssignableTo(Type type)
        {
            var result = new List<Type>();

            foreach (var assembly in GetAssemblies())
            {
                GetTypesAssignableTo(type, assembly)
                    .Where(candidate => result.Find(existing => existing == candidate) == null)
                    .ToList()
                    .ForEach(add => result.Add(add));
            }

            return result;
        }

        public IEnumerable<Type> GetTypesAssignableTo<T>(Assembly assembly)
        {
            return GetTypesAssignableTo(typeof(T), assembly);
        }

        public IEnumerable<Type> GetTypesAssignableTo(Type type, Assembly assembly)
        {
            Guard.AgainstNull(type, nameof(type));
            Guard.AgainstNull(assembly, nameof(assembly));

            return GetTypes(assembly).Where(candidate => candidate.IsAssignableTo(type) && !(candidate.IsInterface && candidate == type)).ToList();
        }

        public IEnumerable<Type> GetTypes(Assembly assembly)
        {
            Type[] types;

            try
            {
                _log.Trace(string.Format(Resources.TraceGetTypesFromAssembly, assembly));

                types = assembly.GetTypes();
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException reflection)
                {
                    foreach (var exception in reflection.LoaderExceptions)
                    {
                        _log.Error($"'{exception.Message}'.");
                    }
                }
                else
                {
                    _log.Error($"{ex.GetType()}: '{ex.Message}'.");
                }

                return new List<Type>();
            }

            return types;
        }
    }
}
