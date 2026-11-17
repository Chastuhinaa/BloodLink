using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Services;

public interface IEligibilityService
{
    Task<EligibilityDto> GetAsync(int donorId, DonationType type, CancellationToken ct = default);
}

public class EligibilityService : IEligibilityService
{
    private readonly BloodLinkDbContext _db;
    private readonly DonationIntervalCalculator _calculator;
    private readonly TimeProvider _time;

    public EligibilityService(BloodLinkDbContext db, DonationIntervalCalculator calculator, TimeProvider time)
    {
        _db = db;
        _calculator = calculator;
        _time = time;
    }

    public async Task<EligibilityDto> GetAsync(int donorId, DonationType type, CancellationToken ct = default)
    {
        var donor = await _db.Donors
            .Include(d => d.Donations)
            .FirstOrDefaultAsync(d => d.Id == donorId, ct)
            ?? throw ServiceException.NotFound("Донора не знайдено.");

        var today = DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);
        var result = _calculator.Calculate(donor.Sex, donor.Donations, type, today);
        return new EligibilityDto(type, result.CanDonateNow, result.NextDate, result.DaysLeft, result.Reason);
    }
}
