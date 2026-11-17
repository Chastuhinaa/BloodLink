using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class DonationServiceTests : IDisposable
{
    private readonly SqliteTestDb _db = new();
    private readonly FixedTimeProvider _time = FixedTimeProvider.On(2026, 11, 15);
    private readonly int _donorId;

    public DonationServiceTests()
    {
        using var ctx = _db.CreateContext();
        var donor = new Donor { Email = "d@test.ua", PasswordHash = "x", FullName = "Тест", BirthDate = new DateOnly(2004, 3, 1) };
        ctx.Donors.Add(donor);
        ctx.SaveChanges();
        _donorId = donor.Id;
    }

    [Fact]
    public async Task Add_ThenList_ReturnsNewestFirst()
    {
        using var ctx = _db.CreateContext();
        var svc = new DonationService(ctx, _time);

        await svc.AddAsync(_donorId, new CreateDonationRequest(new DateOnly(2026, 3, 1), DonationType.WholeBlood, 1, null));
        await svc.AddAsync(_donorId, new CreateDonationRequest(new DateOnly(2026, 9, 1), DonationType.Plasma, null, "  ок  "));

        var list = await svc.ListAsync(_donorId);
        Assert.Equal(2, list.Count);
        Assert.Equal(new DateOnly(2026, 9, 1), list[0].Date);
        Assert.Equal("ок", list[0].Notes);
        Assert.NotNull(list[1].BloodCenterName);
    }

    [Fact]
    public async Task Add_FutureDate_Rejected()
    {
        using var ctx = _db.CreateContext();
        var svc = new DonationService(ctx, _time);

        var ex = await Assert.ThrowsAsync<ServiceException>(() =>
            svc.AddAsync(_donorId, new CreateDonationRequest(new DateOnly(2026, 12, 1), DonationType.WholeBlood, null, null)));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Add_UnknownCenter_Rejected()
    {
        using var ctx = _db.CreateContext();
        var svc = new DonationService(ctx, _time);

        var ex = await Assert.ThrowsAsync<ServiceException>(() =>
            svc.AddAsync(_donorId, new CreateDonationRequest(new DateOnly(2026, 5, 1), DonationType.WholeBlood, 999, null)));

        Assert.Contains("Центр крові не знайдено.", ex.Errors);
    }

    [Fact]
    public async Task Delete_OtherDonorsRecord_NotFound()
    {
        using var ctx = _db.CreateContext();
        var svc = new DonationService(ctx, _time);
        var created = await svc.AddAsync(_donorId, new CreateDonationRequest(new DateOnly(2026, 5, 1), DonationType.WholeBlood, null, null));

        var ex = await Assert.ThrowsAsync<ServiceException>(() => svc.DeleteAsync(_donorId + 100, created.Id));

        Assert.Equal(404, ex.StatusCode);
        Assert.Single(await svc.ListAsync(_donorId));
    }

    public void Dispose() => _db.Dispose();
}
