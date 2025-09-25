namespace Sales.Domain;
public interface IOrderRepository
{
    Task<Order?> GetAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
}
public class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Customer { get; }
    public decimal Amount { get; }
    public Order(string customer, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(customer)) throw new ArgumentException("customer");
        if (amount <= 0) throw new ArgumentException("amount");
        Customer = customer; Amount = amount;
    }
}
