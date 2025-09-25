namespace Sales.Application;
using Sales.Domain;

public record PlaceOrderCommand(string Customer, decimal Amount);

public class PlaceOrderHandler
{
    private readonly IOrderRepository _repo;
    public PlaceOrderHandler(IOrderRepository repo) => _repo = repo;

    public async Task<Guid> Handle(PlaceOrderCommand cmd, CancellationToken ct = default)
    {
        var order = new Order(cmd.Customer, cmd.Amount);
        await _repo.AddAsync(order, ct);
        return order.Id;
    }
}
