using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Services;

public interface IDonationService
{
    Task<IReadOnlyList<DonationDto>> ListAsync(int donorId, CancellationToken ct = default);
    Task<DonationDto> AddAsync(int donorId, CreateDonationRequest request, CancellationToken ct = default);
    Task DeleteAsync(int donorId, int donationId, CancellationToken ct = default);
}

public class DonationService : IDonationService
{
    private readonly BloodLinkDbContext _db;
    private readonly TimeProvider _time;

    public DonationService(BloodLinkDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<IReadOnlyList<DonationDto>> ListAsync(int donorId, CancellationToken ct = default)
    {
        return await _db.Donations
            .AsNoTracking()
            .Where(d => d.DonorId == donorId)
            .OrderByDescending(d => d.Date)
            .ThenByDescending(d => d.Id)
            .Select(d => new DonationDto(d.Id, d.Date, d.Type, d.BloodCenterId,
                d.BloodCenter != null ? d.BloodCenter.Name : null, d.Notes))
            .ToListAsync(ct);
    }

    public async Task<DonationDto> AddAsync(int donorId, CreateDonationRequest request, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var errors = new List<string>();

        if (request.Date > today)
        {
            errors.Add("Дата донації не може бути в майбутньому.");
        }

        if (!Enum.IsDefined(request.Type))
        {
            errors.Add("Невідомий тип донації.");
        }

        var donor = await _db.Donors.FindAsync(new object[] { donorId }, ct)
                    ?? throw ServiceException.NotFound("Донора не знайдено.");

        if (donor.BirthDate is { } bd && request.Date < bd.AddYears(ProfileValidator.MinDonorAge))
        {
            errors.Add("На дату донації донору ще не було 18 років.");
        }

        BloodCenter? center = null;
        if (request.BloodCenterId is { } centerId)
        {
            center = await _db.BloodCenters.FindAsync(new object[] { centerId }, ct);
            if (center is null)
            {
                errors.Add("Центр крові не знайдено.");
            }
        }

        if (errors.Count > 0)
        {
            throw ServiceException.Validation(errors);
        }

        var donation = new Donation
        {
            DonorId = donorId,
            Date = request.Date,
            Type = request.Type,
            BloodCenterId = request.BloodCenterId,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };
        _db.Donations.Add(donation);
        await _db.SaveChangesAsync(ct);

        return new DonationDto(donation.Id, donation.Date, donation.Type, donation.BloodCenterId, center?.Name, donation.Notes);
    }

    public async Task DeleteAsync(int donorId, int donationId, CancellationToken ct = default)
    {
        // Чужий запис і відсутній запис дають однаковий 404, щоб не розкривати чужі дані.
        var donation = await _db.Donations.FirstOrDefaultAsync(d => d.Id == donationId && d.DonorId == donorId, ct)
                       ?? throw ServiceException.NotFound("Запис не знайдено.");
        _db.Donations.Remove(donation);
        await _db.SaveChangesAsync(ct);
    }
}
