using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Shuttle.Core.Reflection.Tests;

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
        Assert.That(GetList().Find<ISomeClass>(), Is.Not.Null);
        Assert.That(GetList().Find<EnumerableExtensionsFixture>(), Is.Null);
    }

    [Test]
    public void Should_be_able_to_find_all_instances()
    {
        Assert.That(GetList().FindAll<ISomeOtherClass>().Count(), Is.EqualTo(2));
        Assert.That(GetList().FindAll<ISomeClass>().Count(), Is.EqualTo(1));
        Assert.That(GetList().FindAll<EnumerableExtensionsFixture>().Count(), Is.EqualTo(0));
    }

    [Test]
    public void Should_be_able_to_get_a_single_instance()
    {
        Assert.That(GetList().Get<ISomeClass>(), Is.Not.Null);
        Assert.Throws<InvalidOperationException>(() => GetList().Get<EnumerableExtensionsFixture>());
    }
}