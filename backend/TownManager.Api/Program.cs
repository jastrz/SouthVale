using System.Text.Json.Serialization;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TownManager.Api.Configuration;
using TownManager.Api.Endpoints;
using TownManager.Api.ExceptionHandling;
using TownManager.Application;
using TownManager.Infrastructure;
using TownManager.Infrastructure.Data;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Filter.ByExcluding(e =>
        e.Properties.TryGetValue("RequestPath", out var path) &&
        path.ToString().Contains("/hangfire"))
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ---------- Options ----------
builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection(CorsOptions.SectionName));

builder.Host.UseSerilog(); 

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// ---------- Application & Infrastructure ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);



// ---------- Authentication ----------
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization();

// ---------- OpenAPI ----------
builder.Services.AddOpenApi();

// ---------- Cross-cutting ----------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// ---------- Middleware pipeline ----------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        await db.Database.EnsureCreatedAsync(); 
        
        if (await userManager.Users.AnyAsync() == false)
        {
            TestDataSeeder testDataSeeder = new(userManager, db);
            await testDataSeeder.SeedAsync();
            Log.Information("Db seeded.");
        }
    }
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ---------- Endpoints ----------
app.MapGet("/", () => Results.Ok(new { name = "TownManager.Api", status = "ok" }))
   .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("Health");

app.UseHangfireDashboard("/hangfire");

app.MapEndpoints();

try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
