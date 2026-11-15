using BloodLink.Api.Dtos;
using BloodLink.Api.Services;
using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class ProfileTests : IDisposable
{
    private static readonly DateOnly Today = new(2026, 11, 15);
    private readonly SqliteTestDb _db = new();

    [Fact]
    public void Validate_CorrectProfile_NoErrors()
    {
        var errors = ProfileValidator.Validate("Олена Коваль", new DateOnly(2003, 5, 10), 50.45, 30.52, Today);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_EmptyName_Error()
    {
        var errors = ProfileValidator.Validate("   ", null, null, null, Today);

        Assert.Contains("Вкажіть ім'я.", errors);
    }

    [Theory]
    [InlineData(2009, 1, 1)]
    [InlineData(1960, 1, 1)]
    [InlineData(2027, 1, 1)]
    public void Validate_AgeOutOfRange_Error(int y, int m, int d)
    {
        var errors = ProfileValidator.Validate("Ім'я", new DateOnly(y, m, d), null, null, Today);

        Assert.Single(errors);
    }

    [Fact]
    public void Validate_OnlyLatitude_Error()
    {
        var errors = ProfileValidator.Validate("Ім'я", null, 50.45, null, Today);

        Assert.Single(errors);
    }

    [Fact]
    public void AgeOn_BeforeBirthday_CountsPreviousYear()
    {
        Assert.Equal(17, ProfileValidator.AgeOn(new DateOnly(2008, 11, 16), Today));
        Assert.Equal(18, ProfileValidator.AgeOn(new DateOnly(2008, 11, 15), Today));
    }

    [Theory]
    [InlineData(BloodGroup.A, RhFactor.Negative, "A(II) Rh−")]
    [InlineData(BloodGroup.O, RhFactor.Positive, "0(I) Rh+")]
    [InlineData(null, null, "не вказано")]
    public void FormatBloodType_ReturnsUkrainianNotation(BloodGroup? g, RhFactor? rh, string expected)
    {
        Assert.Equal(expected, ProfileValidator.FormatBloodType(g, rh));
    }

    [Fact]
    public async Task UpdateAsync_SavesFieldsAndTrimsCity()
    {
        int id;
        using (var ctx = _db.CreateContext())
        {
            var donor = new Donor { Email = "p@test.ua", PasswordHash = "x", FullName = "Старе" };
            ctx.Donors.Add(donor);
            await ctx.SaveChangesAsync();
            id = donor.Id;
        }

        using (var ctx = _db.CreateContext())
        {
            var svc = new ProfileService(ctx, FixedTimeProvider.On(2026, 11, 15));
            var dto = await svc.UpdateAsync(id, new UpdateProfileRequest(
                "Нове Ім'я", Sex.Female, new DateOnly(2004, 1, 1), BloodGroup.B, RhFactor.Negative, "  Київ ", 50.45, 30.52));

            Assert.Equal("B(III) Rh−", dto.BloodType);
            Assert.Equal("Київ", dto.City);
        }

        using (var ctx = _db.CreateContext())
        {
            Assert.Equal(BloodGroup.B, (await ctx.Donors.FindAsync(id))!.BloodGroup);
        }
    }

    [Fact]
    public async Task UpdateAsync_UnknownDonor_NotFound()
    {
        using var ctx = _db.CreateContext();
        var svc = new ProfileService(ctx, FixedTimeProvider.On(2026, 11, 15));

        var ex = await Assert.ThrowsAsync<ServiceException>(() =>
            svc.UpdateAsync(12345, new UpdateProfileRequest("Ім'я", null, null, null, null, null, null, null)));

        Assert.Equal(404, ex.StatusCode);
    }

    public void Dispose() => _db.Dispose();
}
