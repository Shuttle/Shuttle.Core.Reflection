# Shuttle.Core.Reflection

```
PM> Install-Package Shuttle.Core.Reflection
```

Provides various methods to facilitate reflection handling.

## ReflectionService

``` c#
Task<IEnumerable<Assembly>> GetMatchingAssembliesAsync(Regex regex)
```

Returns a collection of assemblies that have their file name matching the given `Regex` expression.

``` c#
Task<IEnumerable<Assembly>> GetRuntimeAssembliesAsync()
```

Returns a combination of `DependencyContext.Default.GetRuntimeAssemblyNames(Environment.OSVersion.Platform.ToString())` and `AppDomain.CurrentDomain.GetAssemblies()`.

``` c#
Task<Type> GetTypeAsync(string typeName)
```

Attempts to find the requested type.

``` c#
Task<IEnumerable<Type>> GetTypesCastableToAsync(Type type, Assembly assembly)
// and these extensions
Task<IEnumerable<Type>> GetTypesCastableToAsync<T>();
Task<IEnumerable<Type>> GetTypesCastableToAsync(Type type);
Task<IEnumerable<Type>> GetTypesCastableToAsync<T>(Assembly assembly);
```

Returns all the types in the given `assembly` that can be cast to the `type` or `typeof(T)`; if no `assembly` is provided the all assemblies returned by `GetAssembliesAsync()` will be scanned.

