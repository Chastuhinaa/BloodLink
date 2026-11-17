using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using BloodLink.Api.Data;
using BloodLink.Api.Services;
using BloodLink.Api.Swagger;
using BloodLink.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton(new DonationIntervalCalculator());
builder.Services.AddSingleton<IPasswordHasher<Donor>, PasswordHasher<Donor>>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<ICenterService, CenterService>();
builder.Services.AddScoped<IDonationService, DonationService>();
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
        Description = "REST API застосунку BloodLink: реєстрація донора, профіль, калькулятор наступної здачі, " +
                      "центри крові за відстанню та історія донацій.",
    });

    var bearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Вставте токен з /api/auth/login (без слова Bearer).",
    };
    c.AddSecurityDefinition("Bearer", bearer);
    c.OperationFilter<AuthorizeOperationFilter>();

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
    {
        c.IncludeXmlComments(xml);
    }
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

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();

app.Run();

// Потрібен для WebApplicationFactory в інтеграційних тестах.
public partial class Program;
