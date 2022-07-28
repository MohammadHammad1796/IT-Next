using System.ComponentModel.DataAnnotations;

namespace IT_Next.Custom.ValidationAttributes;

public class FileTypeImageAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not IFormFile file)
            throw new InvalidCastException();

        var contentType = file.ContentType;
        const StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;
        var allowedTypes = new[]{"image/jpg", "image/jpeg", "image/pjpeg",
                "image/gif", "image/x-png", "image/png"};
        var validType = allowedTypes.Aggregate(false,
            (current, allowedType) =>
                current || string.Equals(contentType, allowedType, comparisonType));

        FormatErrorMessage(validationContext);
        return validType ? ValidationResult.Success : new ValidationResult(ErrorMessage);
    }

    private void FormatErrorMessage(ValidationContext context)
    {
        var displayName = context.DisplayName;
        ErrorMessage = ErrorMessage == null ?
            $"The {displayName} field should be image only." :
            string.Format(ErrorMessage, displayName);
    }
}