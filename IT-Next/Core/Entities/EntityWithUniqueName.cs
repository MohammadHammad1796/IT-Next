namespace IT_Next.Core.Entities;

public abstract class EntityWithUniqueName : BaseEntity
{
    public virtual string Name { get; set; }

    protected EntityWithUniqueName()
    {
    }

    protected EntityWithUniqueName(EntityWithUniqueName entity)
        : base(entity)
    {
    }
}