using System;
using NUnit.Framework;

namespace Shuttle.Core.Reflection.Tests;

[TestFixture]
public class ExceptionExtensionsFixture
{
    [Test]
    public void Should_be_able_to_find_exception()
    {
        var ex = new Exception();

        Assert.That(ex.Find<Exception>(), Is.Not.Null);
        Assert.That(ex.Find<InvalidOperationException>(), Is.Null);
        Assert.That(ex.Contains<Exception>(), Is.True);
        Assert.That(ex.Contains<InvalidOperationException>(), Is.False);

        ex = new(string.Empty, new InvalidOperationException());

        Assert.That(ex.Find<Exception>(), Is.Not.Null);
        Assert.That(ex.Find<InvalidOperationException>(), Is.Not.Null);
        Assert.That(ex.Contains<Exception>(), Is.True);
        Assert.That(ex.Contains<InvalidOperationException>(), Is.True);

        ex = new(string.Empty, new(string.Empty, new InvalidOperationException()));

        Assert.That(ex.Find<InvalidOperationException>(), Is.Not.Null);
        Assert.That(ex.Contains<InvalidOperationException>(), Is.True);
    }
}