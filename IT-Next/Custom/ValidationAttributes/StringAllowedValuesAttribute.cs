using System.ComponentModel.DataAnnotations;

namespace IT_Next.Custom.ValidationAttributes;

/// <summary>
/// Specifies the values of string data allowed in a property.
/// </summary>
[AttributeUsage(validOn: AttributeTargets.Property)]
public class StringAllowedValuesAttribute : ValidationAttribute
{
    private readonly ICollection<string> _allowedValues;

    /// <summary>
    ///     Initializes a new instance of the StringAllowedValuesAttribute
    ///     class based on the type parameter.
    /// </summary>
    /// <param name="type">
    ///     The type allowable values of his properties name data.
    /// </param>
    /// <param name="ignoredProperties">
    ///     Not allowed properties.
    /// </param>
    public StringAllowedValuesAttribute(Type type, params string[] ignoredProperties)
    {
        var propertyInfos = type.GetProperties();
        _allowedValues = propertyInfos.Select(p => p.Name.ToLower()).ToList();

        if (!ignoredProperties.Any())
            return;

        foreach (var ignoredProperty in ignoredProperties)
            if (_allowedValues.Contains(ignoredProperty, StringComparer.CurrentCultureIgnoreCase))
                _allowedValues.Remove(ignoredProperty.ToLower());
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        if (value is not string input)
            throw new InvalidCastException();

        FormatErrorMessage(validationContext.DisplayName);
        var fail = new ValidationResult(ErrorMessage);

        input = input.Trim().ToLower();

        return _allowedValues.Contains(input) ?
            ValidationResult.Success :
            fail;
    }

    private new void FormatErrorMessage(string displayName)
    {
        var joinedAllowedValues = string.Join(", ", _allowedValues);
        ErrorMessage = ErrorMessage == null ?
            $"The {displayName} field allowed values are: {joinedAllowedValues}." :
            string.Format(ErrorMessage, displayName, joinedAllowedValues);
    }
}