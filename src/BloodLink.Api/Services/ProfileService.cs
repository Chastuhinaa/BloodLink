using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;

namespace BloodLink.Api.Services;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(int donorId, CancellationToken ct = default);
    Task<ProfileDto> UpdateAsync(int donorId, UpdateProfileRequest request, CancellationToken ct = default);
}

public class ProfileService : IProfileService
{
    private readonly BloodLinkDbContext _db;
    private readonly TimeProvider _time;

    public ProfileService(BloodLinkDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<ProfileDto> GetAsync(int donorId, CancellationToken ct = default)
    {
        var donor = await FindAsync(donorId, ct);
        return ToDto(donor);
    }

    public async Task<ProfileDto> UpdateAsync(int donorId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var errors = ProfileValidator.Validate(request.FullName, request.BirthDate, request.Latitude, request.Longitude, today);
        if (errors.Count > 0)
        {
            throw ServiceException.Validation(errors);
        }

        var donor = await FindAsync(donorId, ct);
        donor.FullName = request.FullName.Trim();
        donor.Sex = request.Sex;
        donor.BirthDate = request.BirthDate;
        donor.BloodGroup = request.BloodGroup;
        donor.RhFactor = request.RhFactor;
        donor.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        donor.Latitude = request.Latitude;
        donor.Longitude = request.Longitude;

        await _db.SaveChangesAsync(ct);
        return ToDto(donor);
    }

    private async Task<Donor> FindAsync(int donorId, CancellationToken ct) =>
        await _db.Donors.FindAsync(new object[] { donorId }, ct)
        ?? throw ServiceException.NotFound("Донора не знайдено.");

    public static ProfileDto ToDto(Donor d) => new(
        d.Id,
        d.Email,
        d.FullName,
        d.Sex,
        d.BirthDate,
        d.BloodGroup,
        d.RhFactor,
        ProfileValidator.FormatBloodType(d.BloodGroup, d.RhFactor),
        d.City,
        d.Latitude,
        d.Longitude);
}
