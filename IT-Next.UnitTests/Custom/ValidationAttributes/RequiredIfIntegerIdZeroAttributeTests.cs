using IT_Next.Custom.ValidationAttributes;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.UnitTests.Custom.ValidationAttributes;

[TestFixture]
internal class RequiredIfIntegerIdZeroAttributeTests
{
    private RequiredIfIntegerIdZeroAttribute _attribute;
    private TestValidationModel _model;
    private ValidationContext _validationContext;

    [SetUp]
    public void Setup()
    {
        _attribute = new RequiredIfIntegerIdZeroAttribute();
        _model = new TestValidationModel();
        _validationContext = new ValidationContext(_model);
    }

    [Test]
    [TestCase(null, 0, false)]
    [TestCase(null, 1, true)]
    [TestCase("", 0, true)]
    [TestCase("", 1, true)]
    public void IsValid_WhenCalledWithRightModel_ReturnsExpectedResult(string value, int id, bool success)
    {
        _model.Id = id;
        _model.Name = value;

        var result = _attribute.GetValidationResult(_model.Name, _validationContext);

        var equalConstraint = success ?
            Is.EqualTo(ValidationResult.Success) :
            Is.Not.EqualTo(ValidationResult.Success);
        Assert.That(result, equalConstraint);
        Assert.DoesNotThrow(() => _attribute.GetValidationResult(_model.Name, _validationContext));
    }

    [Test]
    public void IsValid_ModelDoesNotHaveIdProperty_ThrowsNullReferenceException()
    {
        var model = new { Name = "Hello" };
        _validationContext = new ValidationContext(model);

        Assert.Throws<NullReferenceException>(() =>
            _attribute.GetValidationResult(model.Name, _validationContext));
    }

    [Test]
    public void IsValid_ModelIdPropertyIsNotInteger_ThrowsInvalidCastException()
    {
        var model = new { Id = "", Name = "Hello" };
        _validationContext = new ValidationContext(model);

        Assert.Throws<InvalidCastException>(() =>
            _attribute.GetValidationResult(model.Name, _validationContext));
    }
}