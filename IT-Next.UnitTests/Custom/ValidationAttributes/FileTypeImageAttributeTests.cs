using IT_Next.Custom.ValidationAttributes;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.UnitTests.Custom.ValidationAttributes
{
    [TestFixture]
    internal class FileTypeImageAttributeTests
    {
        private FileTypeImageAttribute _attribute;
        private TestValidationModel _model;
        private ValidationContext _validationContext;
        private Mock<IFormFile> _formFile;

        [SetUp]
        public void Setup()
        {
            _attribute = new FileTypeImageAttribute();
            _model = new TestValidationModel();
            _validationContext = new ValidationContext(_model);
            _formFile = new Mock<IFormFile>();
        }

        [Test]
        [TestCase("image/jpg", true)]
        [TestCase("image/jpeg", true)]
        [TestCase("image/pjpeg", true)]
        [TestCase("image/gif", true)]
        [TestCase("image/x-png", true)]
        [TestCase("image/png", true)]
        [TestCase("denied", false)]
        [TestCase(null, true)]
        public void IsValid_WhenCalledWithRightModel_ReturnsExpectedResult(string? contentType, bool success)
        {
            _formFile.Setup(f => f.ContentType).Returns(contentType!);
            _model.FormFile = _formFile.Object;
            if (contentType == null)
                _model.FormFile = null;

            var result = _attribute.GetValidationResult(_model.FormFile, _validationContext);

            var equalConstraint = success ?
                Is.EqualTo(ValidationResult.Success) :
                Is.Not.EqualTo(ValidationResult.Success);
            Assert.That(result, equalConstraint);
        }

        [Test]
        public void IsValid_PropertyIsNotFile_ThrowsInvalidCastException()
        {
            var model = new { Id = "", Name = "Hello" };
            _validationContext = new ValidationContext(model);

            Assert.Throws<InvalidCastException>(() =>
                _attribute.GetValidationResult(model.Name, _validationContext));
        }
    }
}