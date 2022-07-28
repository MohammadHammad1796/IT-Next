using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IT_Next.Custom.ValidationAttributes;

/// <summary>
/// Specifies the minimum length of string data allowed in a property if it's not null or whitespace.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Property)]
public class StringNullableOrMinimumLengthAttribute : ValidationAttribute
{
    private readonly uint _minimumLength;

    /// <summary>
    ///     Initializes a new instance of the StringNullableOrMinimumLengthAttribute
    ///     class based on the minimumLength parameter.
    /// </summary>
    /// <param name="minimumLength">
    ///     The minimum allowable length of string data.
    /// </param>
    public StringNullableOrMinimumLengthAttribute(uint minimumLength)
    {
        _minimumLength = minimumLength;
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

        if (input.Length >= _minimumLength)
            return ValidationResult.Success;

        FormatErrorMessage(validationContext.DisplayName);
        return new ValidationResult(ErrorMessage);

    }

    private new void FormatErrorMessage(string displayName)
    {
        ErrorMessage = ErrorMessage == null ?
            $"The field {displayName} minimum length is {_minimumLength}, or should be null." :
            string.Format(ErrorMessage, displayName, _minimumLength);
    }
}