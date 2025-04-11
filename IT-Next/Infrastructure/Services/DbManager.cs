using IT_Next.Core.Services;
using IT_Next.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IT_Next.Infrastructure.Services;

public sealed class DbManager : IDbManager
{
    public async Task<bool> TestConnectionStringAsync(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        var dbContext = new ApplicationDbContext(options);
        var result = await dbContext.Database.CanConnectAsync();
        dbContext.Dispose();
        return result;
    }
}