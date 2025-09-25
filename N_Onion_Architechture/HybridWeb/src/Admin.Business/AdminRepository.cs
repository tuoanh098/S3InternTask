namespace Admin.Business;
using Admin.Data;

public class OrderReportService
{
    private readonly IAdminRepository _repo;
    public OrderReportService(IAdminRepository repo) => _repo = repo;

    public Task<object> ListAsync(CancellationToken ct = default)
        => Task.FromResult<object>(new { totalOrders = _repo.CountOrdersAsync(ct) });
}
