using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IT_Next.Custom.ValidationAttributes;

/// <summary>
/// Specifies the maximum length of string data allowed in a property if it's not null or whitespace.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Property)]
public class StringNullableOrMaximumLengthAttribute : ValidationAttribute
{
    private readonly uint _maximumLength;

    /// <summary>
    ///     Initializes a new instance of the StringNullableOrMaximumLengthAttribute
    ///     class based on the maximumLength parameter.
    /// </summary>
    /// <param name="maximumLength">
    ///     The maximum allowable length of string data.
    /// </param>
    public StringNullableOrMaximumLengthAttribute(uint maximumLength)
    {
        _maximumLength = maximumLength;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not string input)
            throw new InvalidCastException();

        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Success;

        input = input.Trim();
        input = Regex.Replace(input, @"\s+", " ");

        if (input.Length <= _maximumLength)
            return ValidationResult.Success;

        FormatErrorMessage(validationContext.DisplayName);
        return new ValidationResult(ErrorMessage);
    }

    private new void FormatErrorMessage(string displayName)
    {
        ErrorMessage = ErrorMessage == null ?
            $"The field {displayName} maximum length is {_maximumLength}, or should be null." :
            string.Format(ErrorMessage, displayName, _maximumLength);
    }
}