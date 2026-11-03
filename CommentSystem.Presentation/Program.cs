using CommentSystem.Core.Interfaces;
using CommentSystem.Data;
using CommentSystem.Presentation.Hubs;
using CommentSystem.Presentation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

Log.Information("Starting CommentSystem API...");

builder.Services.AddDbContext<CommentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("CommentSystem.Data")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddSingleton<ICommentNotificationService, CommentNotificationService>();
builder.Services.AddHttpClient<ICaptchaVerificationService, CaptchaVerificationService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<CommentCreatedConsumer>();
builder.Services.AddHealthChecks();

//  CORS для SignalR и Web requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://symmetrical-goggles-qvq75g55qjg29jw-3000.app.github.dev"
        )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Host"] ?? "localhost:6379";
    options.InstanceName = "CommentSystem:";
});

var app = builder.Build();

Log.Information("Attempt to migrate database.");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            db.Database.Migrate();
            Log.Information("Database migration applied successfully.");
            break;
        }
        catch (Exception ex) when (attempt < 10)
        {
            Log.Warning("Database not ready (attempt {Attempt}/10): {Message}. Retrying in 5s...", attempt, ex.Message);
            Thread.Sleep(5000);
        }
    }
}
Log.Information("Migration completed.");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CommentSystem API v1");
        options.RoutePrefix = "swagger";
    });
}

app.MapGet("/health", async (CommentDbContext dbContext, IDistributedCache cache, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("HealthCheck");
    var sqlHealthy = false;
    var redisHealthy = false;
    var redisMessage = "";

    try
    {
        sqlHealthy = await dbContext.Database.CanConnectAsync();
        logger.LogInformation("Health check: SQL Server connection {Status}", sqlHealthy ? "OK" : "FAILED");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Health check: SQL Server connection failed");
    }

    try
    {
        var redisKey = "health:check";
        var value = DateTime.UtcNow.ToString("O");
        await cache.SetStringAsync(redisKey, value, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        });

        var result = await cache.GetStringAsync(redisKey);
        redisHealthy = !string.IsNullOrWhiteSpace(result);
        redisMessage = redisHealthy ? "Redis write/read OK" : "Redis read returned empty value";

        logger.LogInformation("Health check: Redis connection {Status}. {Message}", redisHealthy ? "OK" : "FAILED", redisMessage);
    }
    catch (Exception ex)
    {
        redisMessage = ex.Message;
        logger.LogError(ex, "Health check: Redis connection failed");
    }

    var allHealthy = sqlHealthy && redisHealthy;
    return Results.Json(new
    {
        status = allHealthy ? "Healthy" : "Unhealthy",
        sqlServer = sqlHealthy,
        redis = redisHealthy,
        redisMessage,
        timestampUtc = DateTime.UtcNow
    }, statusCode: allHealthy ? 200 : 503);
});

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapHub<CommentHub>("/commenthub");

app.MapControllers();

app.Run();
