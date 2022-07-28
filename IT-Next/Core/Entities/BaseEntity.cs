namespace IT_Next.Core.Entities;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public bool Equals(BaseEntity entity)
    {
        return this.GetType().GetProperties()
            .All(propertyInfo =>
            {
                var firstValue = propertyInfo.GetValue(this);
                var secondValue = propertyInfo.GetValue(entity);
                if (firstValue == null)
                    return secondValue == null;

                return firstValue.Equals(secondValue);
            });
    }

    protected BaseEntity()
    {
    }

    protected BaseEntity(BaseEntity entity)
    {
        foreach (var propertyInfo in entity.GetType().GetProperties())
            propertyInfo.SetValue(this, propertyInfo.GetValue(entity));
    }
}