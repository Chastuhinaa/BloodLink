using System.IdentityModel.Tokens.Jwt;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BloodLink.UnitTests;

public class AuthTests : IDisposable
{
    private const string Key = "unit-test-signing-key-at-least-32-bytes-long";
    private readonly SqliteTestDb _db = new();
    private readonly FixedTimeProvider _time = FixedTimeProvider.On(2026, 11, 15);

    private JwtTokenService Tokens(int minutes = 60) =>
        new(Options.Create(new JwtOptions { Key = Key, ExpiresMinutes = minutes }), _time);

    private AuthService Auth(Api.Data.BloodLinkDbContext ctx) =>
        new(ctx, Tokens(), new PasswordHasher<Donor>());

    [Fact]
    public void CreateToken_ContainsDonorIdAndEmail()
    {
        var (token, _) = Tokens().CreateToken(new Donor { Id = 7, Email = "a@b.ua", FullName = "A" });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("7", jwt.Subject);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "a@b.ua");
        Assert.Equal("BloodLink", jwt.Issuer);
    }

    [Fact]
    public void CreateToken_ExpiresAfterConfiguredMinutes()
    {
        var (_, expires) = Tokens(minutes: 30).CreateToken(new Donor { Id = 1, Email = "a@b.ua", FullName = "A" });

        Assert.Equal(_time.GetUtcNow().UtcDateTime.AddMinutes(30), expires);
    }

    [Fact]
    public void Constructor_ShortKey_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            new JwtTokenService(Options.Create(new JwtOptions { Key = "short" }), _time));
    }

    [Fact]
    public async Task Register_StoresHashedPasswordAndNormalizedEmail()
    {
        using var ctx = _db.CreateContext();

        var resp = await Auth(ctx).RegisterAsync(new RegisterRequest("  Olena@Example.UA ", "Passw0rd!", "Олена"));

        var donor = await ctx.Donors.SingleAsync();
        Assert.Equal("olena@example.ua", donor.Email);
        Assert.NotEqual("Passw0rd!", donor.PasswordHash);
        Assert.False(string.IsNullOrEmpty(resp.Token));
    }

    [Fact]
    public async Task Register_DuplicateEmail_Conflict()
    {
        using var ctx = _db.CreateContext();
        await Auth(ctx).RegisterAsync(new RegisterRequest("dup@test.ua", "Passw0rd!", "Перша"));

        var ex = await Assert.ThrowsAsync<ServiceException>(() =>
            Auth(ctx).RegisterAsync(new RegisterRequest("DUP@test.ua", "Other123!", "Друга")));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsToken()
    {
        using var ctx = _db.CreateContext();
        await Auth(ctx).RegisterAsync(new RegisterRequest("login@test.ua", "Passw0rd!", "Донор"));

        var resp = await Auth(ctx).LoginAsync(new LoginRequest("login@test.ua", "Passw0rd!"));

        Assert.Equal("Донор", resp.FullName);
    }

    [Theory]
    [InlineData("login@test.ua", "WrongPass1")]
    [InlineData("nobody@test.ua", "Passw0rd!")]
    public async Task Login_WrongCredentials_Unauthorized(string email, string password)
    {
        using var ctx = _db.CreateContext();
        await Auth(ctx).RegisterAsync(new RegisterRequest("login@test.ua", "Passw0rd!", "Донор"));

        var ex = await Assert.ThrowsAsync<ServiceException>(() => Auth(ctx).LoginAsync(new LoginRequest(email, password)));

        Assert.Equal(401, ex.StatusCode);
    }

    public void Dispose() => _db.Dispose();
}
