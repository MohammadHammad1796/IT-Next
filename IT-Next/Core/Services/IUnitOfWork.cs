namespace IT_Next.Core.Services;

public interface IUnitOfWork
{
    Task CompleteAsync();
}