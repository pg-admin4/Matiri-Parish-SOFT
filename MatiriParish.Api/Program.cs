using MatiriParish.Core.Data;
using MatiriParish.Core.Services;
using MatiriParish.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Connection string: prefer configuration, then environment variable, otherwise a local placeholder.
var conn = builder.Configuration.GetConnectionString("Default")
           ?? Environment.GetEnvironmentVariable("ConnectionStrings__Default")
           ?? "Host=localhost;Port=5432;Database=matiri_parish_api;Username=postgres;Password=postgres";

// EF Core DbContextFactory for the shared Core project
builder.Services.AddDbContextFactory<MatiriParishDbContext>(opts => opts.UseNpgsql(conn));

// Also register API DbContext (uses same connection) so controllers can inject it directly
builder.Services.AddMemoryCache();

// Configure optional Redis distributed cache when Redis connection string is provided
var redisConfig = builder.Configuration["Redis:Configuration"];
if (!string.IsNullOrWhiteSpace(redisConfig))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConfig;
    });
}

builder.Services.AddDbContext<MatiriParish.Api.Data.MatiriParishDbContext>(opts =>
{
    opts.UseNpgsql(conn);
    // Enable detailed EF logging in development to help surface slow queries (do not enable in production)
    if (builder.Environment.IsDevelopment())
    {
        opts.EnableSensitiveDataLogging();
        opts.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }
});

// Minimal key/value store for server (replace with Redis or similar in production)
builder.Services.AddSingleton<IKeyValueStore, InMemoryKeyValueStore>();

// Register core AuthService using the factory + kv store
builder.Services.AddScoped<IAuthService>(sp =>
{
    var factory = sp.GetRequiredService<IDbContextFactory<MatiriParishDbContext>>();
    var kv = sp.GetRequiredService<IKeyValueStore>();
    return new AuthService(factory, kv);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MatiriParish API", Version = "v1" });
});

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ReplaceThisWithARealSecretKeyOfSufficientLength";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MatiriParish.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MatiriParishClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// Ensure database exists / apply migrations at startup (safe for demo). Catch errors so the server still starts when DB is not available.
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MatiriParishDbContext>>();
        // Create a DbContext instance and apply migrations
        using var db = dbFactory.CreateDbContext();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration failed at startup: {ex.Message}");
}

// Seed sample data when in development or when explicitly enabled via configuration
try
{
    var seedEnabled = builder.Configuration.GetValue<bool?>("Seed:Enable") ?? false;
    if (app.Environment.IsDevelopment() || seedEnabled)
    {
        using var scope = app.Services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MatiriParishDbContext>>();
        using var db = dbFactory.CreateDbContext();

        // Idempotent seed: create a dev admin user if not exists
        var existing = await db.Set<MatiriParish.Core.Models.User>().FirstOrDefaultAsync(u => u.Username == "dev_admin");
        if (existing == null)
        {
            // create a simple seeded admin account with known password (for dev only)
            var saltBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes("Dev@1234", saltBytes, 120_000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

            var user = new MatiriParish.Core.Models.User
            {
                Username = "dev_admin",
                PasswordSalt = salt,
                PasswordHash = hash,
                Role = MatiriParish.Core.Models.UserRole.Admin,
                Iterations = 120_000,
                PasswordHashAlgorithm = "PBKDF2-SHA256-v1"
            };
            db.Set<MatiriParish.Core.Models.User>().Add(user);
            await db.SaveChangesAsync();
            Console.WriteLine("Seed: created dev_admin user (password: Dev@1234)");
        }

        // Create a few sample persons with a baptism record tagged as sample:dev
        var existsSample = await db.Persons.AnyAsync(p => p.FirstName.StartsWith("DEV_") || (p.Baptism != null && p.Baptism.Remarks != null && p.Baptism.Remarks.Contains("sample:dev")));
        if (!existsSample)
        {
            var person = new MatiriParish.Api.Models.Person
            {
                FirstName = "DEV_John",
                MiddleName = "",
                LastName = "Doe",
                DateOfBirth = DateTime.UtcNow.AddYears(-30)
            };
            db.Persons.Add(person);
            await db.SaveChangesAsync();

            var baptism = new MatiriParish.Api.Models.BaptismRecord
            {
                PersonId = person.Id,
                DateOfBaptism = DateTime.UtcNow.AddYears(-29),
                OfficiatingPriest = "Fr. Sample",
                BookNumber = "S1",
                PageNumber = 1,
                EntryNumber = 1,
                Remarks = "sample:dev"
            };
            db.Baptisms.Add(baptism);
            await db.SaveChangesAsync();

            Console.WriteLine($"Seed: created sample person id={person.Id}");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Seeding failed: {ex.Message}");
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Simple in-memory key/value store implementation used by server-side AuthService
public class InMemoryKeyValueStore : IKeyValueStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _store = new();

    public System.Threading.Tasks.Task<string?> GetAsync(string key)
    {
        _store.TryGetValue(key, out var v);
        return System.Threading.Tasks.Task.FromResult<string?>(v);
    }

    public System.Threading.Tasks.Task SetAsync(string key, string value)
    {
        _store[key] = value;
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task RemoveAsync(string key)
    {
        _store.TryRemove(key, out _);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
