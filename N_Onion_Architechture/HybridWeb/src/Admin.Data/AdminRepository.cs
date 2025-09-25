using Microsoft.EntityFrameworkCore;

namespace Admin.Data;

public interface IAdminRepository
{
    Task<int> CountOrdersAsync(CancellationToken ct = default);
}

public class AdminRepository : IAdminRepository
{
    private readonly AdminDbContext _db;
    public AdminRepository(AdminDbContext db) => _db = db;

    public Task<int> CountOrdersAsync(CancellationToken ct = default)
        => _db.Orders.CountAsync(ct);
}
