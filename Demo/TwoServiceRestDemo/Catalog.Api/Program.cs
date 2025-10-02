using Catalog.Api;
using NLog;
using NLog.Web;
using Shared.Contracts;

var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()   // đọc nlog.config
                       .GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Log HTTP in/out (đổ vào ILogger -> NLog)
    builder.Services.AddHttpLogging(o =>
    {
        o.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration;
    });

    var app = builder.Build();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpLogging();

    // Correlation Id (ghi vào scope để NLog lấy ${scopeproperty:CorrelationId})
    app.Use(async (ctx, next) =>
    {
        var cid = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                  ?? Guid.NewGuid().ToString("n");
        ctx.Response.Headers["X-Correlation-Id"] = cid;
        using (app.Logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = cid }))
        {
            await next();
        }
    });

    var seed = new[]
    {
    new BookDto(1, "Clean Architecture", 39.90m, 25),
    new BookDto(2, "Deep Work",          18.75m, 20),
    new BookDto(3, "Kubernetes in Action", 49.00m, 10)
    };

    // Đăng ký service in-memory
    builder.Services.AddSingleton<IBookCatalog>(new InMemoryBookCatalog(seed));

    app.MapGet("/api/catalog", async (IBookCatalog catalog, CancellationToken ct) =>
    {
        var items = await catalog.GetAllAsync(ct);
        return Results.Ok(items);
    })
    .WithName("GetCatalog")
    .WithOpenApi();

    app.MapGet("/api/catalog/books/{id:long}", async (long id, IBookCatalog catalog, CancellationToken ct) =>
    {
        var b = await catalog.GetByIdAsync(id, ct);
        return b is null ? Results.NotFound() : Results.Ok(b);
    })
    .WithName("GetBook")
    .WithOpenApi();
}
catch (Exception ex)
{
    logger.Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
public partial class Program { }