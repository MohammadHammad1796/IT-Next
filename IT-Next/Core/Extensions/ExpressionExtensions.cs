using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace IT_Next.Core.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<TClass, object>> GenerateExpressionPropertySelector<TClass>(
        string propertyName) where TClass : class
    {
        var classType = typeof(TClass);
        var propertyInfo = classType
            .GetProperties()
            .FirstOrDefault(x => x.Name
                .Equals(propertyName, StringComparison.CurrentCultureIgnoreCase));
        if (propertyInfo == null)
            throw new NullReferenceException();

        var parameterExpression = Parameter(classType, "x");
        var propertyExpression = Property(parameterExpression, propertyName);
        var convertedPropertyExpression = Convert(propertyExpression, typeof(object));
        var expression = Lambda<Func<TClass, object>>(convertedPropertyExpression, parameterExpression);

        return expression;
    }

    public static Expression<Func<TClass, bool>> AppendOr<TClass>(
        this Expression<Func<TClass, bool>> left, Expression<Func<TClass, bool>> right)
        where TClass : class
    {
        var parameter = Parameter(typeof(TClass), "x");
        var body = OrElse(left.Body, right.Body);
        body = (BinaryExpression)new ParameterReplacer(parameter).Visit(body);

        var predicate = Lambda<Func<TClass, bool>>(body, parameter);
        return predicate;
    }

    public static Expression<Func<TClass, bool>> AppendAnd<TClass>(
        this Expression<Func<TClass, bool>> left, Expression<Func<TClass, bool>> right)
        where TClass : class
    {
        var parameter = Parameter(typeof(TClass), "x");
        var body = AndAlso(left.Body, right.Body);
        body = (BinaryExpression)new ParameterReplacer(parameter).Visit(body);

        var predicate = Lambda<Func<TClass, bool>>(body, parameter);
        return predicate;
    }

    public static Expression<Func<TClass, bool>> GenerateOrConditionsPredicate<TClass>(
        string value) where TClass : class
    {
        var classType = typeof(TClass);
        var classProperties = classType.GetProperties();
        if (!classProperties.Any())
            throw new Exception("");

        var parameterExpression = Parameter(classType, "x");

        var boolExpressions = new List<Expression<Func<TClass, bool>>>();
        boolExpressions.AddRange(
            GeneratePropertiesExpressions<TClass>(value, nameof(string.Contains), parameterExpression));

        var isInt = int.TryParse(value, out var intValue);
        if (isInt)
            boolExpressions.AddRange(
                GeneratePropertiesExpressions<TClass>(intValue, nameof(int.Equals), parameterExpression));

        var isFloat = float.TryParse(value, out var floatValue);
        if (isFloat)
            boolExpressions.AddRange(
                GeneratePropertiesExpressions<TClass>(floatValue, nameof(bool.Equals), parameterExpression));

        if (!boolExpressions.Any())
            throw new Exception("");

        var predicate = GenerateOrElsePredicateFromExpressions(boolExpressions);

        return (Expression<Func<TClass, bool>>)predicate;
    }

    private static ICollection<Expression<Func<TClass, bool>>> GeneratePropertiesExpressions<TClass>(
        object value, string methodName,
        ParameterExpression parameterExpression) where TClass : class
    {
        var type = value.GetType();
        var properties = typeof(TClass)
            .GetProperties()
            .Where(x => x.PropertyType == type && !x.Name.Contains("image", StringComparison.CurrentCultureIgnoreCase));

        var methodInfo = type.GetMethod(methodName, new[] { type });
        var searchValue = Constant(value, type);
        var predicates = GenerateExpressions<TClass>(
            properties, parameterExpression,
            searchValue, methodInfo!);
        return predicates;
    }

    private static ICollection<Expression<Func<TClass, bool>>> GenerateExpressions<TClass>(
        IEnumerable<PropertyInfo> properties,
        ParameterExpression parameterExpression,
        Expression searchValue,
        MethodInfo methodInfo) where TClass : class
    {
        return (from property in properties
                select Property(parameterExpression, property)
            into propertyExpression
                select Call(propertyExpression, methodInfo, searchValue)
            into searchMethodExpression
                select Lambda<Func<TClass, bool>>(searchMethodExpression, parameterExpression)).ToList();
    }

    private static Expression GenerateOrElsePredicateFromExpressions<TClass>(
        IReadOnlyCollection<Expression<Func<TClass, bool>>> expressions) where TClass : class
    {
        Expression predicate = expressions.ElementAt(0);
        for (var i = 1; i < expressions.Count; i++)
            predicate = ((Expression<Func<TClass, bool>>)predicate)
                .AppendOr(expressions.ElementAt(i));

        return predicate;
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        protected override Expression VisitParameter(ParameterExpression node)
            => base.VisitParameter(_parameter);

        internal ParameterReplacer(ParameterExpression parameter)
            => _parameter = parameter;
    }
}

