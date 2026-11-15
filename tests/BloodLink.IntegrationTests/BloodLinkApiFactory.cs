using BloodLink.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BloodLink.IntegrationTests;

public class BloodLinkApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Development");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("ConnectionStrings:BloodLink", "DataSource=:memory:");
        builder.UseSetting("Jwt:Key", "integration-test-signing-key-at-least-32-bytes");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BloodLinkDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<BloodLinkDbContext>(o => o.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
