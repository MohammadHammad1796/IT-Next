using IT_Next.Core.Extensions;
using NUnit.Framework;

namespace IT_Next.UnitTests.Core.Extensions;

[TestFixture]
internal class StringExtensionsTests
{
    [Test]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("   ", "")]
    [TestCase(" hello ", "hello")]
    [TestCase(" hello   world  ", "hello world")]
    public void TrimExtraSpaces_WhenCalled_ReturnsExpectedValue(string? value, string expectedResult)
    {
        var result = value.TrimExtraSpaces();

        Assert.That(expectedResult, Is.EqualTo(result));
    }
}