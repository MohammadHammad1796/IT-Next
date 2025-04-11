using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.Custom.ValidationAttributes;

public class RequiredIfIntegerIdZeroAttribute : ValidationAttribute, IClientModelValidator
{
     protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var idProperty = validationContext.ObjectType.GetProperty("Id")?
            .GetValue(validationContext.ObjectInstance);
        if (idProperty == null)
            throw new NullReferenceException();

        if (idProperty is not int id)
            throw new InvalidCastException();

        FormatErrorMessage(validationContext);
        if (value == null && id == 0)
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }

    private void FormatErrorMessage(ValidationContext context)
    {
        var displayName = context.DisplayName;
        ErrorMessage = ErrorMessage == null ?
            $"The {displayName} field is required." :
            string.Format(ErrorMessage, displayName);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-requiredifintegeridzero", FormatErrorMessage(context.ModelMetadata.GetDisplayName()) ?? $"The field is required.");
    }

    private static bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (attributes.ContainsKey(key))
            return false;

        attributes.Add(key, value);
        return true;
    }
}