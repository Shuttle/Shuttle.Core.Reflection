using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Core.Reflection.Tests;

[TestFixture]
public class ReflectionServiceFixture
{
    [Test]
    public async Task Should_be_able_to_get_runtime_assemblies_async()
    {
        Assert.That((await new ReflectionService().GetRuntimeAssembliesAsync()).Count(), Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_be_able_to_get_types_async()
    {
        Assert.That((await new ReflectionService().GetTypesCastableToAsync<SomeClass>()).Count(), Is.EqualTo(1));
        Assert.That((await new ReflectionService().GetTypesCastableToAsync<ISomeClass>()).Count(), Is.EqualTo(1));
    }

    public async Task Should_be_able_to_get_matching_assemblies_async()
    {
        var service = new ReflectionService();

        Assert.That((await service.GetMatchingAssembliesAsync(".+")).Count(), Is.GreaterThan(0));

        foreach (var assembly in await service.GetMatchingAssembliesAsync("nunit"))
        {
            Assert.That(assembly.FullName!.Contains("nunit"));
        }
    }
}