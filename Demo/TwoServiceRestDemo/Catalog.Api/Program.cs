using Shared.Contracts;
using NLog;
using NLog.Web;

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