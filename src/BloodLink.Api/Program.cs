using System.Text.Json.Serialization;
using BloodLink.Api.Data;
using BloodLink.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
var connectionString = builder.Configuration.GetConnectionString("BloodLink")
                       ?? throw new InvalidOperationException("Не задано ConnectionStrings:BloodLink.");
builder.Services.AddDbContext<BloodLinkDbContext>(options =>
{
    if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

builder.Services.AddExceptionHandler<ServiceExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BloodLink API",
        Version = "v1",
    });
});

var app = builder.Build();

// Схема БД створюється під час старту (для модуля 2; міграції — на весну).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<BloodLinkDbContext>().Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BloodLink API v1");
    c.DocumentTitle = "BloodLink API";
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();

app.Run();

// Потрібен для WebApplicationFactory в інтеграційних тестах.
public partial class Program;
