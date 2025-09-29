using System.Net.Http.Json;
using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Read base URL of Catalog service
var catalogBase = builder.Configuration["Catalog:BaseUrl"]
                  ?? throw new InvalidOperationException("Missing Catalog:BaseUrl");

// Register a typed HttpClient for Catalog
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(c =>
{
    c.BaseAddress = new Uri(catalogBase);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Demo application service (very thin)
builder.Services.AddScoped<CheckoutService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Endpoint: quote an order total by calling Catalog over HTTP
app.MapGet("/api/orders/quote/{bookId:long}/{qty:int}", async (long bookId, int qty, CheckoutService svc) =>
{
    try
    {
        var total = await svc.QuoteAsync(bookId, qty);
        return Results.Ok(new { bookId, qty, total });
    }
    catch (ArgumentOutOfRangeException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
})
.WithName("QuoteOrder")
.WithOpenApi();

app.Run();


interface ICatalogClient
{
    Task<BookDto?> GetBookAsync(long id, CancellationToken ct = default);
}

// HTTP implementation (REST call to Catalog)
class CatalogClient(HttpClient http) : ICatalogClient
{
    public Task<BookDto?> GetBookAsync(long id, CancellationToken ct = default) =>
        http.GetFromJsonAsync<BookDto>($"/api/catalog/books/{id}", ct);
}

class CheckoutService(ICatalogClient catalog)
{
    public async Task<decimal> QuoteAsync(long bookId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty), "Quantity must be > 0");

        var book = await catalog.GetBookAsync(bookId, ct)
                   ?? throw new InvalidOperationException("Book not found");

        if (book.StockQty < qty) throw new InvalidOperationException("Insufficient stock");

        return book.Price * qty;
    }
}
