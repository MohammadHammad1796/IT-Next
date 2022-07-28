using IT_Next.Custom.ValidationAttributes;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IT_Next.UnitTests.Custom.ValidationAttributes;

[TestFixture]
internal class StringAllowedValuesAttributeTests
{
    private StringAllowedValuesAttribute _attribute;
    private TestValidationModel _model;
    private ValidationContext _validationContext;

    [SetUp]
    public void Setup()
    {
        _attribute = new StringAllowedValuesAttribute(typeof(TestValidationModel), "formfile");
        _model = new TestValidationModel();
        _validationContext = new ValidationContext(_model);
    }

    [Test]
    [TestCase("id", true)]
    [TestCase("nAmE", true)]
    [TestCase("PRICE", true)]
    [TestCase(" price ", true)]
    [TestCase(null, true)]
    [TestCase("na me", false)]
    [TestCase("cost", false)]
    [TestCase("formfile", false)]
    public void IsValid_WhenCalledWithRightProperty_ReturnsExpectedResult(string value, bool success)
    {
        _model.Name = value;

        var result = _attribute.GetValidationResult(_model.Name, _validationContext);

        if (success)
            Assert.That(result, Is.EqualTo(ValidationResult.Success));
        else
        {
            Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
            Assert.That(result, Is.Not.Null);
            var allowedValues = typeof(TestValidationModel).GetProperties()
                .Where(p => p.Name.ToLower() != "formfile")
                .Select(p => p.Name.ToLower());
            foreach (var allowedValue in allowedValues)
                Assert.That(result!.ErrorMessage!.Contains(allowedValue));
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