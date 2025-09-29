using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Đăng ký Ocelot
builder.Services.AddOcelot();

// (Dev) mở CORS cho dễ thử
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapGet("/health", () => Results.Ok(new { ok = true, ts = DateTime.UtcNow }));

// Chạy reverse proxy
await app.UseOcelot();

app.Run();
