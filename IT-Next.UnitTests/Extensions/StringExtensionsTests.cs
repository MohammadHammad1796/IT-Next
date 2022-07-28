using IT_Next.Extensions;
using NUnit.Framework;

namespace IT_Next.UnitTests.Extensions;

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

        Assert.AreEqual(expectedResult, result);
    }
}