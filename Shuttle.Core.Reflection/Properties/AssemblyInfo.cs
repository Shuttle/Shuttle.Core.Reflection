using System.Reflection;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
[assembly: AssemblyTitle(".NET Framework")]
#endif

#if NETCOREAPP
[assembly: AssemblyTitle(".NET Core")]
#endif

#if NETSTANDARD
[assembly: AssemblyTitle(".NET Standard")]
#endif

[assembly: AssemblyVersion("10.0.11.0")]
[assembly: AssemblyCopyright("Copyright (c) 2020, Eben Roux")]
[assembly: AssemblyProduct("Shuttle.Core.Reflection")]
[assembly: AssemblyCompany("Eben Roux")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyInformationalVersion("10.0.11")]
[assembly: ComVisible(false)]