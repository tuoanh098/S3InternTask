using Microsoft.EntityFrameworkCore;
using Sales.Infrastructure;
using Admin.Data;

var builder = WebApplication.CreateBuilder(args);

var salesConn = builder.Configuration.GetConnectionString("Sales")
    ?? "Server=localhost;Port=3306;Database=salesdb;User=tuoanh;Password=Tuoanh098186@;CharSet=utf8mb4;";
var adminConn = builder.Configuration.GetConnectionString("Admin")
    ?? "Server=localhost;Port=3306;Database=admindb;User=tuoanh;Password=Tuoanh098186@;CharSet=utf8mb4;";

builder.Services.AddDbContext<SalesDbContext>(opt =>
    opt.UseMySql(salesConn, ServerVersion.AutoDetect(salesConn)));
builder.Services.AddDbContext<AdminDbContext>(opt =>
    opt.UseMySql(adminConn, ServerVersion.AutoDetect(adminConn)));

var app = builder.Build();
app.Run();
