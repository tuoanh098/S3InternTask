using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using NLog;
using NLog.Web;

var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()   // đọc nlog.config
                       .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Đọc cấu hình Ocelot
    builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

    // Đăng ký Ocelot
    builder.Services.AddOcelot();

    // (Dev) mở CORS cho dễ thử
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Log HTTP in/out (đổ vào ILogger -> NLog)
    builder.Services.AddHttpLogging(o =>
    {
        o.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode
                        | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.Duration;
    });

    var app = builder.Build();

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

    app.UseCors();
    app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));

    // Chạy reverse proxy
    await app.UseOcelot();

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