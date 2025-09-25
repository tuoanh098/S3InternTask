using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- MySQL (Pomelo) ---
var cs = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(cs, ServerVersion.AutoDetect(cs), mySql => mySql.EnableRetryOnFailure()));

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();

// Friendly root
app.MapGet("/", () => Results.Redirect("/swagger"));

// CRUD demo
app.MapGet("/students", async (AppDbContext db) => await db.Students.AsNoTracking().ToListAsync());

app.MapGet("/students/{id:int}", async (AppDbContext db, int id) =>
{
    var s = await db.Students.FindAsync(id);
    return s is null ? Results.NotFound() : Results.Ok(s);
});

app.MapPost("/students", async (AppDbContext db, Student s) =>
{
    db.Students.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/students/{s.Id}", s);
});

// Concurrency-aware update (unchanged from earlier)
app.MapPut("/students/{id:int}", async (AppDbContext db, int id, Student input) =>
{
    var existing = await db.Students.AsTracking().FirstOrDefaultAsync(s => s.Id == id);
    if (existing is null) return Results.NotFound();

    existing.FullName = input.FullName;
    existing.Email = input.Email;

    try { await db.SaveChangesAsync(); return Results.Ok(existing); }
    catch (DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.Single();
        var databaseValues = await entry.GetDatabaseValuesAsync();
        if (databaseValues is null) return Results.Problem("The record was deleted by another user.", statusCode: 409);

        return Results.Json(new
        {
            message = "Concurrency conflict",
            client = entry.CurrentValues.ToObject(),
            server = databaseValues.ToObject(),
            resolution = "Decide which values to keep and retry"
        }, statusCode: 409);
    }
});

app.Run();
