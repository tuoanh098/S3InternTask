using Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In-memory catalog (no database to keep demo simple)
var books = new List<BookDto>
{
    new(1, "Clean Architecture", 39.90m, 25),
    new(2, "Deep Work",          18.75m, 20),
    new(3, "Kubernetes in Action", 49.00m, 10)
};

app.MapGet("/api/catalog", () => Results.Ok(books))
   .WithName("GetCatalog")
   .WithOpenApi();

app.MapGet("/api/catalog/books/{id:long}", (long id) =>
{
    var b = books.FirstOrDefault(x => x.Id == id);
    return b is null ? Results.NotFound() : Results.Ok(b);
})
.WithName("GetBook")
.WithOpenApi();

app.Run();
