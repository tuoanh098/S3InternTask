using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// ======== Options ========
var jwtOpt = new JwtOptions();
builder.Configuration.GetSection("Jwt").Bind(jwtOpt);

// ======== Services ========
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtDemo", Version = "v1" });

    // --- Add Bearer auth to Swagger ---
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT here (no need to type 'Bearer ')."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton(jwtOpt);
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();
builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOpt.Issuer,
            ValidAudience = jwtOpt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.Key)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.UniqueName
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection(); // ok over HTTP too, but HTTPS recommended

app.UseAuthentication();
app.UseAuthorization();

// ======== Endpoints ========

// Health
app.MapGet("/", () => Results.Ok(new { status = "ok", ts = DateTime.UtcNow }));

// Login -> issues access token (body) + refresh token (HttpOnly cookie) + (also return refresh for Postman)
app.MapPost("/api/auth/login", (LoginRequest req, IUserStore users, ITokenService tokens, JwtOptions opt, HttpContext http) =>
{
    var user = users.ValidateCredentials(req.UserName, req.Password);
    if (user is null) return Results.Unauthorized();

    var access = tokens.CreateAccessToken(user);
    var rt = tokens.CreateRefreshToken(opt.RefreshTokenDays, http.Connection.RemoteIpAddress?.ToString());
    users.AddRefreshToken(user, rt);

    // Cookie flags: Lax works for Swagger & Postman on same-origin; set None+Secure for SPA cross-site
    var cookie = new CookieOptions
    {
        HttpOnly = true,
        Secure = http.Request.IsHttps,          // true on HTTPS
        SameSite = http.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
        Expires = rt.Expires
    };
    http.Response.Cookies.Append("refreshToken", rt.Token, cookie);

    return Results.Ok(new LoginResponse
    {
        AccessToken = access,
        ExpiresIn = opt.AccessTokenMinutes * 60,
        // For Postman convenience (NOT for web apps): also return refresh token
        RefreshToken = rt.Token,
        RefreshExpires = rt.Expires
    });
});

// Refresh -> uses cookie if present; otherwise accepts body { refreshToken }
app.MapPost("/api/auth/refresh", (RefreshRequest? body, IUserStore users, ITokenService tokens, JwtOptions opt, HttpContext http) =>
{
    var token = http.Request.Cookies["refreshToken"];
    if (string.IsNullOrWhiteSpace(token))
        token = body?.RefreshToken;

    if (string.IsNullOrWhiteSpace(token))
        return Results.Unauthorized();

    var (u, rt) = users.FindActiveRefreshToken(token);
    if (u is null || rt is null) return Results.Unauthorized();

    // rotate
    var newRt = tokens.CreateRefreshToken(opt.RefreshTokenDays, http.Connection.RemoteIpAddress?.ToString());
    users.AddRefreshToken(u, newRt);
    users.RevokeToken(rt.Token, http.Connection.RemoteIpAddress?.ToString(), replacedBy: newRt.Token);

    var cookie = new CookieOptions
    {
        HttpOnly = true,
        Secure = http.Request.IsHttps,
        SameSite = http.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
        Expires = newRt.Expires
    };
    http.Response.Cookies.Append("refreshToken", newRt.Token, cookie);

    var access = tokens.CreateAccessToken(u);
    return Results.Ok(new LoginResponse
    {
        AccessToken = access,
        ExpiresIn = opt.AccessTokenMinutes * 60,
        RefreshToken = newRt.Token,      // Postman convenience
        RefreshExpires = newRt.Expires
    });
});

// Logout -> revoke and clear cookie
app.MapPost("/api/auth/logout", (IUserStore users, HttpContext http) =>
{
    if (http.Request.Cookies.TryGetValue("refreshToken", out var token))
        users.RevokeToken(token, http.Connection.RemoteIpAddress?.ToString());
    http.Response.Cookies.Delete("refreshToken");
    return Results.NoContent();
});

// Protected whoami
app.MapGet("/api/auth/me", [Authorize] (ClaimsPrincipal user) =>
{
    var name = user.Identity?.Name ?? user.FindFirstValue(JwtRegisteredClaimNames.UniqueName);
    return Results.Ok(new { user = name });
});

// Another protected sample
app.MapGet("/api/secure/hello", [Authorize] () =>
{
    return Results.Ok(new { message = "Hello, authorized user!" });
});

app.Run();


// ======== Types & Services ========

public record LoginRequest(string UserName, string Password);
public record RefreshRequest(string? RefreshToken);
public record LoginResponse
{
    public string AccessToken { get; set; } = "";
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }           // for Postman convenience
    public DateTime? RefreshExpires { get; set; }
}

public class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 5;
    public int RefreshTokenDays { get; set; } = 7;
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = "";
    public string Password { get; set; } = ""; // demo only (plain text). Use hashed passwords in real apps.
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

public class RefreshToken
{
    public string Token { get; set; } = "";
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime? Revoked { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsExpired && !IsRevoked;
}

public interface IUserStore
{
    User? ValidateCredentials(string userName, string password);
    (User? user, RefreshToken? token) FindActiveRefreshToken(string token);
    void AddRefreshToken(User user, RefreshToken rt);
    void RevokeToken(string token, string? ip, string? replacedBy = null);
}

public class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryUserStore()
    {
        // demo user
        var demo = new User { UserName = "demo", Password = "Pass@123" };
        _users[demo.UserName] = demo;
    }

    public User? ValidateCredentials(string userName, string password)
        => _users.TryGetValue(userName, out var u) && u.Password == password ? u : null;

    public (User? user, RefreshToken? token) FindActiveRefreshToken(string token)
    {
        foreach (var u in _users.Values)
        {
            var rt = u.RefreshTokens.FirstOrDefault(t => t.Token == token);
            if (rt is not null && rt.IsActive) return (u, rt);
        }
        return (null, null);
    }

    public void AddRefreshToken(User user, RefreshToken rt) => user.RefreshTokens.Add(rt);

    public void RevokeToken(string token, string? ip, string? replacedBy = null)
    {
        foreach (var u in _users.Values)
        {
            var rt = u.RefreshTokens.FirstOrDefault(t => t.Token == token);
            if (rt is null) continue;
            rt.Revoked = DateTime.UtcNow;
            rt.RevokedByIp = ip;
            rt.ReplacedByToken = replacedBy;
            return;
        }
    }
}

public interface ITokenService
{
    string CreateAccessToken(User user);
    RefreshToken CreateRefreshToken(int days, string? ip = null);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _opt;
    private readonly SymmetricSecurityKey _key;

    public TokenService(JwtOptions opt)
    {
        _opt = opt;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
    }

    public string CreateAccessToken(User user)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken CreateRefreshToken(int days, string? ip = null)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(bytes),
            Expires = DateTime.UtcNow.AddDays(days),
            Created = DateTime.UtcNow,
            CreatedByIp = ip
        };
    }
}
