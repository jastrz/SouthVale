using Scalar.AspNetCore;
using TownManager.Api.Configuration;
using TownManager.Api.Endpoints;
using TownManager.Api.ExceptionHandling;
using TownManager.Application;
using TownManager.Infrastructure;
using TownManager.Infrastructure.Identity;
using TownManager.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---------- Options ----------
builder.Services
    .AddOptions<CorsOptions>()
    .Bind(builder.Configuration.GetSection(CorsOptions.SectionName));

// ---------- Application & Infrastructure ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---------- Authentication ----------

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
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
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ---------- Endpoints ----------
app.MapGet("/", () => Results.Ok(new { name = "TownManager.Api", status = "ok" }))
   .WithName("Root");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithName("Health");

app.MapGroup("/identity")
    .MapIdentityApi<ApplicationUser>();

app.MapEndpoints();

app.Run();
