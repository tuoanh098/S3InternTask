namespace Sales.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Sales.Domain;

public class EfOrderRepository : IOrderRepository
{
    private readonly SalesDbContext _db;
    public EfOrderRepository(SalesDbContext db) => _db = db;

    public Task<Order?> GetAsync(Guid id, CancellationToken ct = default)
        => _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
    }
}
    