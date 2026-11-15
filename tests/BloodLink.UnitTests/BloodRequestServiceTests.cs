using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class BloodRequestServiceTests : IDisposable
{
    private readonly SqliteTestDb _db = new();
    private readonly FixedTimeProvider _time = FixedTimeProvider.On(2026, 11, 15);
    private readonly int _authorId;
    private readonly int _donorOnegId;
    private readonly int _donorApId;

    public BloodRequestServiceTests()
    {
        using var ctx = _db.CreateContext();
        var author = new Donor { Email = "author@test.ua", PasswordHash = "x", FullName = "Родич пацієнта" };
        var oneg = new Donor { Email = "oneg@test.ua", PasswordHash = "x", FullName = "Донор 0-", Sex = Sex.Male, BloodGroup = BloodGroup.O, RhFactor = RhFactor.Negative, Latitude = 50.45, Longitude = 30.52 };
        var ap = new Donor { Email = "ap@test.ua", PasswordHash = "x", FullName = "Донор A+", Sex = Sex.Female, BloodGroup = BloodGroup.A, RhFactor = RhFactor.Positive };
        ctx.Donors.AddRange(author, oneg, ap);
        ctx.SaveChanges();
        _authorId = author.Id;
        _donorOnegId = oneg.Id;
        _donorApId = ap.Id;
    }

    private BloodRequestService Service(BloodLinkDbContext ctx) => new(ctx, new DonationIntervalCalculator(), _time);

    private static CreateBloodRequestRequest NewRequest(BloodGroup g = BloodGroup.A, RhFactor rh = RhFactor.Negative,
        string phone = "+380671234567", RequestUrgency urgency = RequestUrgency.Urgent, int centerId = 1) =>
        new(g, rh, DonationType.WholeBlood, 2, centerId, urgency, phone, "О. К.", "Лікарня №1", null, new DateOnly(2026, 11, 30));

    [Fact]
    public async Task Create_SavesRequest_AndAuthorSeesPhone()
    {
        using var ctx = _db.CreateContext();

        var dto = await Service(ctx).CreateAsync(_authorId, NewRequest());

        Assert.Equal("A(II) Rh−", dto.BloodType);
        Assert.True(dto.IsMine);
        Assert.Equal("+380671234567", dto.ContactPhone);
        Assert.Equal(RequestStatus.Open, dto.Status);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("phone-number")]
    public async Task Create_InvalidPhone_Rejected(string phone)
    {
        using var ctx = _db.CreateContext();

        var ex = await Assert.ThrowsAsync<ServiceException>(() => Service(ctx).CreateAsync(_authorId, NewRequest(phone: phone)));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Create_UnknownCenter_Rejected()
    {
        using var ctx = _db.CreateContext();

        var ex = await Assert.ThrowsAsync<ServiceException>(() => Service(ctx).CreateAsync(_authorId, NewRequest(centerId: 999)));

        Assert.Contains("Центр крові не знайдено.", ex.Errors);
    }

    [Fact]
    public async Task List_HidesPhone_AndMarksWhoCanHelp()
    {
        using var ctx = _db.CreateContext();
        var svc = Service(ctx);
        await svc.CreateAsync(_authorId, NewRequest(BloodGroup.A, RhFactor.Negative));

        var forOneg = (await svc.ListOpenAsync(_donorOnegId, null, null, null, false)).Single();
        var forAp = (await svc.ListOpenAsync(_donorApId, null, null, null, false)).Single();

        Assert.Null(forOneg.ContactPhone);
        Assert.True(forOneg.CanHelp);
        Assert.NotNull(forOneg.DistanceKm);
        Assert.False(forAp.CanHelp);
        Assert.Contains("не підходить", forAp.CannotHelpReason);
    }

    [Fact]
    public async Task List_SortsCriticalFirst_AndFiltersCompatible()
    {
        using var ctx = _db.CreateContext();
        var svc = Service(ctx);
        await svc.CreateAsync(_authorId, NewRequest(BloodGroup.B, RhFactor.Positive, urgency: RequestUrgency.Normal));
        await svc.CreateAsync(_authorId, NewRequest(BloodGroup.O, RhFactor.Positive, urgency: RequestUrgency.Critical));
        await svc.CreateAsync(_authorId, NewRequest(BloodGroup.A, RhFactor.Positive, urgency: RequestUrgency.Urgent));

        var all = await svc.ListOpenAsync(_donorApId, null, null, null, false);
        var compatible = await svc.ListOpenAsync(_donorApId, null, null, null, true);

        Assert.Equal(RequestUrgency.Critical, all[0].Urgency);
        Assert.Equal(BloodGroup.A, Assert.Single(compatible).BloodGroup);
    }

    [Fact]
    public async Task Respond_RevealsPhone_AndAuthorSeesResponder()
    {
        using var ctx = _db.CreateContext();
        var svc = Service(ctx);
        var created = await svc.CreateAsync(_authorId, NewRequest());

        var afterRespond = await svc.RespondAsync(_donorOnegId, created.Id);
        var forAuthor = (await svc.ListMineAsync(_authorId)).Single();

        Assert.True(afterRespond.IResponded);
        Assert.Equal("+380671234567", afterRespond.ContactPhone);
        Assert.Equal("Донор 0-", Assert.Single(forAuthor.Responders!).FullName);
    }

    [Fact]
    public async Task Respond_IncompatibleOrOwnOrTwice_Rejected()
    {
        using var ctx = _db.CreateContext();
        var svc = Service(ctx);
        var created = await svc.CreateAsync(_authorId, NewRequest(BloodGroup.A, RhFactor.Negative));
        await svc.RespondAsync(_donorOnegId, created.Id);

        var incompatible = await Assert.ThrowsAsync<ServiceException>(() => svc.RespondAsync(_donorApId, created.Id));
        var own = await Assert.ThrowsAsync<ServiceException>(() => svc.RespondAsync(_authorId, created.Id));
        var twice = await Assert.ThrowsAsync<ServiceException>(() => svc.RespondAsync(_donorOnegId, created.Id));

        Assert.Equal(400, incompatible.StatusCode);
        Assert.Equal(400, own.StatusCode);
        Assert.Equal(409, twice.StatusCode);
    }

    [Fact]
    public async Task Respond_DonorNotYetEligible_Rejected()
    {
        using (var ctx = _db.CreateContext())
        {
            ctx.Donations.Add(new Donation { DonorId = _donorOnegId, Date = new DateOnly(2026, 11, 1), Type = DonationType.WholeBlood });
            await ctx.SaveChangesAsync();
        }

        using var ctx2 = _db.CreateContext();
        var svc = Service(ctx2);
        var created = await svc.CreateAsync(_authorId, NewRequest());

        var ex = await Assert.ThrowsAsync<ServiceException>(() => svc.RespondAsync(_donorOnegId, created.Id));

        Assert.Contains("Ви зможете здавати з", ex.Message);
    }

    [Fact]
    public async Task UpdateStatus_OnlyAuthor_AndClosedRequestDisappearsFromList()
    {
        using var ctx = _db.CreateContext();
        var svc = Service(ctx);
        var created = await svc.CreateAsync(_authorId, NewRequest());

        var notAuthor = await Assert.ThrowsAsync<ServiceException>(() => svc.UpdateStatusAsync(_donorOnegId, created.Id, RequestStatus.Fulfilled));
        await svc.UpdateStatusAsync(_authorId, created.Id, RequestStatus.Fulfilled);

        Assert.Equal(404, notAuthor.StatusCode);
        Assert.Empty(await svc.ListOpenAsync(_donorOnegId, null, null, null, false));
    }

    public void Dispose() => _db.Dispose();
}
