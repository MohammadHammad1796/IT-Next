using System.Collections.Generic;
using System.Linq;
using IT_Next.Core.Extensions;
using IT_Next.Core.Helpers;
using NUnit.Framework;

namespace IT_Next.UnitTests.Core.Extensions;

[TestFixture]
internal class QueryableExtensionsTests
{
    private List<Entity> _list;

    [SetUp]
    public void SetUp()
    {
        _list = new List<Entity>();
        var charCode = 65;
        for (var i = 0; i < 10; i++)
        {
            var name = ((char)charCode++).ToString();
            _list.Add(new Entity { Id = i, Name = name });
            name = ((char)charCode++).ToString();
            _list.Add(new Entity { Id = i, Name = name });
        }
    }

    [Test]
    public void ApplyPaging_WhenCalled_ReturnExpectedResult()
    {
        var expectedQuery = _list.AsQueryable();
        expectedQuery = expectedQuery.Skip(0).Take(5);
        var extensionQuery = _list.AsQueryable();

        extensionQuery = extensionQuery.ApplyPaging(new Paging(1, 5));

        Assert.That(expectedQuery.First().Id, Is.EqualTo(extensionQuery.First().Id));
        Assert.That(expectedQuery.Last().Id, Is.EqualTo(extensionQuery.Last().Id));
    }

    [Test]
    public void ApplyOrders_WHenOrderingListIsEmpty_ReturnsSameQueryable()
    {
        var expectedQuery = _list.AsQueryable();
        var extensionQuery = _list.AsQueryable();

        extensionQuery = extensionQuery.ApplyOrders(new List<Ordering<Entity>>());

        Assert.That(extensionQuery.Count(), Is.EqualTo(expectedQuery.Count()));
    }

    [Test]
    public void ApplyOrders_WHenOrderingListIsNotEmpty_ReturnsExpectedOrderedQueryable()
    {
        var extensionQuery = _list.AsQueryable();
        var orderings = new List<Ordering<Entity>>
        {
            new(orderBy: e => e.Id, isAscending: true),
            new(orderBy: e => e.Name, isAscending: false)
        };

        var expectedQuery = extensionQuery.ApplyOrders(orderings);

        Assert.That(expectedQuery.ElementAt(0).Id, Is.EqualTo(0));
        Assert.That(expectedQuery.ElementAt(0).Name, Is.EqualTo("B"));
        Assert.That(expectedQuery.ElementAt(1).Id, Is.EqualTo(0));
        Assert.That(expectedQuery.ElementAt(1).Name, Is.EqualTo("A"));
    }

    private class Entity
    {
        public int Id { get; init; }
        public string Name { get; init; }
    }
}