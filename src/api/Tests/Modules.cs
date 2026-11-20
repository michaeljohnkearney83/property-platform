using System.Reflection;

namespace PropertyPlatform.Tests;

/// <summary>The module assemblies the Host loads. Shared by the architecture tests.</summary>
internal static class Modules
{
    public static readonly Assembly[] Assemblies =
        typeof(PropertyPlatform.Host.ModuleCatalog).Assembly
            .GetReferencedAssemblies()
            .Where(a => a.Name!.StartsWith("PropertyPlatform.Modules.", StringComparison.Ordinal))
            .Select(Assembly.Load)
            .ToArray();
}
