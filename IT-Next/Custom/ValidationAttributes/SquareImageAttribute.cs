using System.ComponentModel.DataAnnotations;
using SixLabors.ImageSharp;

namespace IT_Next.Custom.ValidationAttributes;

public class SquareImageAttribute : ValidationAttribute
{
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
            return image.Width == image.Height ? ValidationResult.Success : new ValidationResult(ErrorMessage);
        }
        catch (Exception)
        {
            return new ValidationResult(ErrorMessage);
        }
    }

    private void FormatErrorMessage()
    {
        ErrorMessage = ErrorMessage == null ?
            $"The image should be square (same width and height)." :
            ErrorMessage;
    }
}