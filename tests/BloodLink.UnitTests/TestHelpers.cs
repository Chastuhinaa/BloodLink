using BloodLink.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.UnitTests;

public sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedTimeProvider(DateTimeOffset now) => _now = now;

    public static FixedTimeProvider On(int year, int month, int day) =>
        new(new DateTimeOffset(year, month, day, 12, 0, 0, TimeSpan.Zero));

    public override DateTimeOffset GetUtcNow() => _now;
}

public sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public BloodLinkDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<BloodLinkDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
