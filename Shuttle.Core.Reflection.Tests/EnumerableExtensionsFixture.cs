using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Shuttle.Core.Reflection.Tests
{
    [TestFixture]
    public class EnumerableExtensionsFixture
    {
        private static IEnumerable<object> GetList()
        {
            return new List<object>
            {
                new SomeClass(),
                new SomeOtherClass(),
                new SomeOtherClass()
            };
        }

        [Test]
        public void Should_be_able_to_find_a_single_instance()
        {
            Assert.IsNotNull(GetList().Find<ISomeClass>());
            Assert.IsNull(GetList().Find<EnumerableExtensionsFixture>());
        }

        [Test]
        public void Should_be_able_to_find_all_instances()
        {
            Assert.AreEqual(2, GetList().FindAll<ISomeOtherClass>().Count());
            Assert.AreEqual(1, GetList().FindAll<ISomeClass>().Count());
            Assert.AreEqual(0, GetList().FindAll<EnumerableExtensionsFixture>().Count());
        }

        [Test]
        public void Should_be_able_to_get_a_single_instance()
        {
            Assert.IsNotNull(GetList().Get<ISomeClass>());
            Assert.Throws<InvalidOperationException>(() => GetList().Get<EnumerableExtensionsFixture>());
        }
    }
}