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
internal class ValidateModelFilterTests
{
    private ValidateModelFilter _filter;
    private ActionExecutingContext _context;
    private Mock<ActionExecutionDelegate> _next;

    [SetUp]
    public void SetUp()
    {
        _filter = new ValidateModelFilter();
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
        _context = new ActionExecutingContext(actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!);
        _next = new Mock<ActionExecutionDelegate>();
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task OnActionExecutionAsync_WhenCalled_ReturnExpectedResult(bool isValid)
    {
        if (!isValid)
            _context.ModelState.AddModelError("", "");

        await _filter.OnActionExecutionAsync(_context, _next.Object);

        if (isValid)
            Assert.That(_context.Result, Is.Not.TypeOf<BadRequestObjectResult>());
        else
        {
            Assert.That(_context.Result, Is.TypeOf<BadRequestObjectResult>());
            var objectResult = _context.Result as BadRequestObjectResult;
            Assert.That(objectResult, Is.Not.Null);
        }
    }
}