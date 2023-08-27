using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Shuttle.Core.Reflection.Tests
{
    [TestFixture]
    public class ReflectionServiceFixture
    {
        [Test]
        public void Should_be_able_to_get_runtime_assemblies()
        {
            Assert.That(new ReflectionService().GetRuntimeAssemblies().Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task Should_be_able_to_get_runtime_assemblies_async()
        {
            Assert.That((await new ReflectionService().GetRuntimeAssembliesAsync()).Count(), Is.GreaterThan(0));
        }

        [Test]
        public void Should_be_able_to_get_types()
        {
            Assert.That(new ReflectionService().GetTypesAssignableTo<SomeClass>().Count(), Is.EqualTo(1));
            Assert.That(new ReflectionService().GetTypesAssignableTo<ISomeClass>().Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Should_be_able_to_get_types_async()
        {
            Assert.That((await new ReflectionService().GetTypesAssignableToAsync<SomeClass>()).Count(), Is.EqualTo(1));
            Assert.That((await new ReflectionService().GetTypesAssignableToAsync<ISomeClass>()).Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_be_able_to_get_matching_assemblies()
        {
            var service = new ReflectionService();

            Assert.That(service.GetMatchingAssemblies(".+").Count(), Is.GreaterThan(0));

            foreach (var assembly in service.GetMatchingAssemblies("nunit"))
            {
                Assert.That(assembly.FullName.Contains("nunit"));
            }
        }

        public async Task Should_be_able_to_get_matching_assemblies_async()
        {
            var service = new ReflectionService();
            
            Assert.That((await service.GetMatchingAssembliesAsync(".+")).Count(), Is.GreaterThan(0));

            foreach (var assembly in (await service.GetMatchingAssembliesAsync("nunit")))
            {
                Assert.That(assembly.FullName.Contains("nunit"));
            }
        }
    }
}