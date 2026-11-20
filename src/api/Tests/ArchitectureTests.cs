using System.Reflection;
using PropertyPlatform.SharedKernel;

namespace PropertyPlatform.Tests;

public class ArchitectureTests
{
    private static readonly Assembly[] ModuleAssemblies = Modules.Assemblies;

    [Fact]
    public void All_modules_are_registered_in_the_host()
    {
        Assert.Equal(ModuleAssemblies.Length, PropertyPlatform.Host.ModuleCatalog.All.Count);
    }

    [Fact]
    public void Modules_do_not_reference_each_other()
    {
        foreach (var module in ModuleAssemblies)
        {
            var offending = module.GetReferencedAssemblies()
                .Where(r => r.Name!.StartsWith("PropertyPlatform.Modules.", StringComparison.Ordinal))
                .Select(r => r.Name)
                .ToList();

            Assert.True(offending.Count == 0, $"{module.GetName().Name} references {string.Join(", ", offending)}. Talk through SharedKernel contracts or events instead.");
        }
    }

    [Fact]
    public void Every_module_exposes_exactly_one_IModule()
    {
        foreach (var module in ModuleAssemblies)
        {
            var count = module.GetTypes().Count(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract);
            Assert.True(count == 1, $"{module.GetName().Name} has {count} IModule implementations.");
        }
    }
}
