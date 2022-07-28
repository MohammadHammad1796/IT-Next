using IT_Next.Custom.ValidationAttributes;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.UnitTests.Custom.ValidationAttributes;

[TestFixture]
internal class StringNullableOrMaximumLengthAttributeTests
{
    private StringNullableOrMaximumLengthAttribute _attribute;
    private TestValidationModel _model;
    private ValidationContext _validationContext;

    [SetUp]
    public void Setup()
    {
        _attribute = new StringNullableOrMaximumLengthAttribute(2);
        _model = new TestValidationModel();
        _validationContext = new ValidationContext(_model);
    }

    [Test]
    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("     ", true)]
    [TestCase("a", true)]
    [TestCase("a ", true)]
    [TestCase("ab", true)]
    [TestCase("abc", false)]
    public void IsValid_WhenCalledWithRightProperty_ReturnsExpectedResult(string value, bool success)
    {
        _model.Name = value;

        var result = _attribute.GetValidationResult(_model.Name, _validationContext);

        if (success)
            Assert.That(result, Is.EqualTo(ValidationResult.Success));
        else
        {
            Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
            Assert.That(result!.ErrorMessage!.Contains('2'));
        }
        Assert.DoesNotThrow(() => _attribute.GetValidationResult(_model.Name, _validationContext));
    }

    [Test]
    public void IsValid_PropertyIsNotString_ThrowsInvalidCastException()
    {
        _model.Id = 1;

        Assert.Throws<InvalidCastException>(() =>
            _attribute.GetValidationResult(_model.Id, _validationContext));
    }
}