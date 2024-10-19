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

For .Net 4.6+ (which isn't support in the latest version) returns `AppDomain.CurrentDomain.GetAssemblies();`.  For .Net Core 2.0+ all the `DependencyContext.Default.GetRuntimeAssemblyNames(RuntimeEnvironment.GetRuntimeIdentifier())` assembly names are resolved.

``` c#
Task<Type> GetTypeAsync(string typeName)
```

Attempts to find the requested type.

``` c#
Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type, Assembly assembly)
// and these extensions
Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>();
Task<IEnumerable<Type>> GetTypesAssignableToAsync(Type type);
Task<IEnumerable<Type>> GetTypesAssignableToAsync<T>(Assembly assembly);
```

Returns all the types in the given `assembly` that are assignable to the `type` or `typeof(T)`; if no `assembly` is provided the all assemblies returned by `GetAssemblies()` will be scanned.

