using IT_Next.Custom.FilterAttributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IT_Next.UnitTests.Custom.FilterAttributes;

[TestFixture]
internal class TrimStringsFilterTests
{
    private TrimStringsFilter _filter;
    private ActionExecutingContext _context;
    private Mock<ActionExecutionDelegate> _next;
    private TestModel _model1;
    private TestModel _model2;
    private TestModel? _model3;

    [SetUp]
    public void SetUp()
    {
        _filter = new TrimStringsFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        _model1 = new TestModel { Id = 0, FirstName = null!, LastName = "" };
        _model2 = new TestModel { Id = 0, FirstName = "       ", LastName = "  test   extra" };
        _model3 = null;
        var actionArguments = new Dictionary<string, object?>
        {
            {"a", _model1},
            {"b", _model2},
            {"c", _model3}
        };
        _context = new ActionExecutingContext(actionContext,
            new List<IFilterMetadata>(),
            actionArguments,
            null!);
        _next = new Mock<ActionExecutionDelegate>();
    }

    [Test]
    public async Task OnActionExecutionAsync_WhenCalled_TrimStringsAndExtraSpaces()
    {
        await _filter.OnActionExecutionAsync(_context, _next.Object);

        Assert.That(_model1.FirstName, Is.EqualTo(string.Empty));
        Assert.That(_model1.LastName, Is.EqualTo(string.Empty));
        Assert.That(_model2.FirstName, Is.EqualTo(string.Empty));
        Assert.That(_model2.LastName, Is.EqualTo("test extra"));
        Assert.That(_model3, Is.Null);
    }

    private class TestModel
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int Id { get; set; }

        public string FirstName { get; init; }

        public string LastName { get; init; }
    }
}