using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;

namespace IT_Next.Custom.ValidationAttributes;

public class MinimumImageWidthAttribute : ValidationAttribute
{
    private readonly int _minimumWidth;

    public MinimumImageWidthAttribute(int minimumWidth) => _minimumWidth = minimumWidth;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not IFormFile file)
            throw new InvalidCastException();

        FormatErrorMessage();
        try
        {
            var image = Image.Load(file.OpenReadStream());
            return image.Width >= _minimumWidth ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
        catch (Exception)
        {
            return new ValidationResult(ErrorMessage);
        }
    }

    private void FormatErrorMessage()
    {
        ErrorMessage = ErrorMessage == null ?
            $"The image width should be at least {_minimumWidth} pixel." :
            string.Format(ErrorMessage, _minimumWidth);
    }
}