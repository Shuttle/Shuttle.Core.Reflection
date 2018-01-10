using System.Linq;
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
        public void Should_be_able_to_get_types()
        {
            Assert.That(new ReflectionService().GetTypesAssignableTo<SomeClass>().Count(), Is.EqualTo(1));
            Assert.That(new ReflectionService().GetTypesAssignableTo<ISomeClass>().Count(), Is.EqualTo(1));
        }
    }
}