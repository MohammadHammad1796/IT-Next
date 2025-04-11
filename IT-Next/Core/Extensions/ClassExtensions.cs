using IT_Next.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Extensions;

public static class ClassExtensions
{
    public static ResultWithResponse<List<ValidationResult>> Validate<TModel>(this TModel model) where TModel : class
    {
        var context = new ValidationContext(model);
        var errorResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, errorResults, true);
        return new ResultWithResponse<List<ValidationResult>>(isValid, errorResults);
    }
}