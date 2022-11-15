using IT_Next.Core.Entities;
using IT_Next.Core.Repositories;
using IT_Next.Infrastructure.Data;

namespace IT_Next.Infrastructure.Repositories;

public class ContactMessageRepository : Repository<ContactMessage>, IContactMessageRepository
{
    public ContactMessageRepository(ApplicationDbContext dbContext,
        IQueryCustomization<ContactMessage> queryCustomization)
        : base(dbContext, queryCustomization)
    {
    }
}