using IT_Next.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static IT_Next.Extensions.ExpressionExtensions;

namespace IT_Next.UnitTests.Extensions;

[TestFixture]
internal class ExpressionExtensionsTests
{
    private List<TestModel> _list;

    [SetUp]
    public void SetUp()
    {
        _list = new List<TestModel>
            {
                new() {Id = 1, FirstName = "abc", LastName = "", HavePassport = false},
                new() {Id = 2, FirstName = "1", LastName = ""},
                new() {Id = 3, FirstName = "true", LastName = "abc"},
                new() {Id = 4, FirstName = "abc", LastName = "1", HavePassport = true},
                new() {Id = 1, FirstName = "abc", LastName = ""}
            };
    }

    [Test]
    public void GenerateExpressionPropertySelector_PropertyExistedInClass_ReturnsExpressionAndWorkFine()
    {
        Expression<Func<TestModel, object>> expression = x => x.Id;

        var result = GenerateExpressionPropertySelector<TestModel>("id");
        var selectResults = _list.Select(result.Compile());

        Assert.That(result.ToString(), Is.EqualTo(expression.ToString()));
        Assert.That(selectResults.ElementAt(0), Is.EqualTo(1));
    }

    [Test]
    public void GenerateExpressionPropertySelector_PropertyDoesNotExistedInClass_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() =>
            GenerateExpressionPropertySelector<TestModel>("fake"));
    }

    [Test]
    [TestCase("bc", 1, 3)]
    [TestCase("ba", 1, 2)]
    [TestCase("abcd", 5, 0)]
    public void AppendOr_WhenCalled_CombinePredicatesInNewOne(string str, int number, int expectedCount)
    {
        Expression<Func<TestModel, bool>> left = x => x.FirstName.Contains(str);
        Expression<Func<TestModel, bool>> right = c => c.Id.Equals(number);

        var result = left.AppendOr(right);
        var listResult = _list.Where(result.Compile());

        Assert.That(listResult.Count(), Is.EqualTo(expectedCount));
    }

    [Test]
    [TestCase("1", 4)]
    [TestCase("bc", 4)]
    public void GenerateOrConditionsPredicate_ClassHaveStringAndIntegerProperties_ReturnPredicateAndWorkFine(
        string value, int expectedCount)
    {
        var result = GenerateOrConditionsPredicate<TestModel>(value);
        var conditionResults = _list.Where(result.Compile());

        Assert.That(conditionResults.Count(), Is.EqualTo(expectedCount));
    }

    [Test]
    public void GenerateOrConditionsPredicate_ClassDoesNotHaveProperties_ThrowsException()
    {
        Assert.Throws<Exception>(() => GenerateOrConditionsPredicate<EmptyModel>("fake"));
    }

    [Test]
    public void GenerateOrConditionsPredicate_ClassDoesNotHaveStringAndIntegerProperties_ThrowsException()
    {
        Assert.Throws<Exception>(() => GenerateOrConditionsPredicate<NoneIntAndString>("fake"));
    }

    internal class TestModel
    {
        public int Id { get; init; }

        public string FirstName { get; init; }

        public DateOnly BirthDate { get; set; }

        public string LastName { get; init; }

        public bool HavePassport { get; set; }
    }
    internal abstract class EmptyModel { }

    internal abstract class NoneIntAndString
    {
        public DateOnly BirthDate { get; set; }
    }
}