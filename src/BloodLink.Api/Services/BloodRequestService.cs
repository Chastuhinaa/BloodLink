using System.Text.RegularExpressions;
using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Services;

public interface IBloodRequestService
{
    Task<IReadOnlyList<BloodRequestDto>> ListOpenAsync(int userId, double? latitude, double? longitude, double? radiusKm, bool onlyCompatible, CancellationToken ct = default);
    Task<IReadOnlyList<BloodRequestDto>> ListMineAsync(int userId, CancellationToken ct = default);
    Task<BloodRequestDto> GetAsync(int userId, int requestId, CancellationToken ct = default);
    Task<BloodRequestDto> CreateAsync(int userId, CreateBloodRequestRequest request, CancellationToken ct = default);
    Task<BloodRequestDto> RespondAsync(int userId, int requestId, CancellationToken ct = default);
    Task CancelResponseAsync(int userId, int requestId, CancellationToken ct = default);
    Task<BloodRequestDto> UpdateStatusAsync(int userId, int requestId, RequestStatus status, CancellationToken ct = default);
}

public partial class BloodRequestService : IBloodRequestService
{
    private readonly BloodLinkDbContext _db;
    private readonly DonationIntervalCalculator _calculator;
    private readonly TimeProvider _time;

    public BloodRequestService(BloodLinkDbContext db, DonationIntervalCalculator calculator, TimeProvider time)
    {
        _db = db;
        _calculator = calculator;
        _time = time;
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetUtcNow().UtcDateTime);

    public async Task<IReadOnlyList<BloodRequestDto>> ListOpenAsync(
        int userId, double? latitude, double? longitude, double? radiusKm, bool onlyCompatible, CancellationToken ct = default)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw ServiceException.Validation(new[] { "Вкажіть і широту (lat), і довготу (lng)." });
        }

        if (radiusKm is <= 0)
        {
            throw ServiceException.Validation(new[] { "Радіус має бути додатним." });
        }

        var user = await LoadUserAsync(userId, ct);
        var lat = latitude ?? user.Latitude;
        var lng = longitude ?? user.Longitude;
        var today = Today;

        var requests = await Query()
            .Where(r => r.Status == RequestStatus.Open)
            .ToListAsync(ct);

        var items = requests
            .Where(r => r.NeededBy is null || r.NeededBy >= today)
            .Select(r => ToDto(r, user, Distance(r, lat, lng)))
            .Where(d => radiusKm is null || d.DistanceKm is null || d.DistanceKm <= radiusKm)
            .Where(d => !onlyCompatible || d.CanHelp || d.IsMine)
            .OrderByDescending(d => d.Urgency)
            .ThenBy(d => d.DistanceKm ?? double.MaxValue)
            .ThenByDescending(d => d.CreatedAt)
            .ToList();

        return items;
    }

    public async Task<IReadOnlyList<BloodRequestDto>> ListMineAsync(int userId, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);
        var requests = await Query()
            .Where(r => r.AuthorId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return requests.Select(r => ToDto(r, user, null)).ToList();
    }

    public async Task<BloodRequestDto> GetAsync(int userId, int requestId, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);
        var request = await FindAsync(requestId, ct);
        return ToDto(request, user, Distance(request, user.Latitude, user.Longitude));
    }

    public async Task<BloodRequestDto> CreateAsync(int userId, CreateBloodRequestRequest request, CancellationToken ct = default)
    {
        var errors = new List<string>();
        if (!Enum.IsDefined(request.BloodGroup) || !Enum.IsDefined(request.RhFactor))
        {
            errors.Add("Некоректна група крові.");
        }

        var type = request.Type ?? DonationType.WholeBlood;
        var urgency = request.Urgency ?? RequestUrgency.Normal;
        if (!Enum.IsDefined(type) || !Enum.IsDefined(urgency))
        {
            errors.Add("Некоректний тип донації або терміновість.");
        }

        if (!IsValidPhone(request.ContactPhone))
        {
            errors.Add("Телефон має містити від 10 до 15 цифр, напр. +380671234567.");
        }

        if (request.NeededBy is { } neededBy && neededBy < Today)
        {
            errors.Add("Дата «потрібно до» не може бути в минулому.");
        }

        if (!await _db.BloodCenters.AnyAsync(c => c.Id == request.BloodCenterId, ct))
        {
            errors.Add("Центр крові не знайдено.");
        }

        if (errors.Count > 0)
        {
            throw ServiceException.Validation(errors);
        }

        var entity = new BloodRequest
        {
            AuthorId = userId,
            BloodGroup = request.BloodGroup,
            RhFactor = request.RhFactor,
            Type = type,
            UnitsNeeded = request.UnitsNeeded ?? 1,
            BloodCenterId = request.BloodCenterId,
            Urgency = urgency,
            ContactPhone = request.ContactPhone.Trim(),
            PatientName = Clean(request.PatientName),
            Hospital = Clean(request.Hospital),
            Description = Clean(request.Description),
            NeededBy = request.NeededBy,
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        };
        _db.BloodRequests.Add(entity);
        await _db.SaveChangesAsync(ct);

        return await GetAsync(userId, entity.Id, ct);
    }

    public async Task<BloodRequestDto> RespondAsync(int userId, int requestId, CancellationToken ct = default)
    {
        var user = await LoadUserAsync(userId, ct);
        var request = await FindAsync(requestId, ct);

        if (request.AuthorId == userId)
        {
            throw ServiceException.Validation(new[] { "Не можна відгукнутися на власний запит." });
        }

        if (request.Status != RequestStatus.Open)
        {
            throw ServiceException.Validation(new[] { "Запит уже закритий." });
        }

        if (request.Responses.Any(r => r.DonorId == userId))
        {
            throw ServiceException.Conflict("Ви вже відгукнулися на цей запит.");
        }

        var (canHelp, reason) = CheckCanHelp(request, user);
        if (!canHelp)
        {
            throw ServiceException.Validation(new[] { reason! });
        }

        _db.RequestResponses.Add(new RequestResponse
        {
            RequestId = requestId,
            DonorId = userId,
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        });
        await _db.SaveChangesAsync(ct);

        return await GetAsync(userId, requestId, ct);
    }

    public async Task CancelResponseAsync(int userId, int requestId, CancellationToken ct = default)
    {
        var response = await _db.RequestResponses.FirstOrDefaultAsync(r => r.RequestId == requestId && r.DonorId == userId, ct)
                       ?? throw ServiceException.NotFound("Відгук не знайдено.");
        _db.RequestResponses.Remove(response);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<BloodRequestDto> UpdateStatusAsync(int userId, int requestId, RequestStatus status, CancellationToken ct = default)
    {
        var request = await _db.BloodRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.AuthorId == userId, ct)
                      ?? throw ServiceException.NotFound("Запит не знайдено.");

        if (!Enum.IsDefined(status))
        {
            throw ServiceException.Validation(new[] { "Некоректний статус." });
        }

        request.Status = status;
        await _db.SaveChangesAsync(ct);
        return await GetAsync(userId, requestId, ct);
    }

    public (bool CanHelp, string? Reason) CheckCanHelp(BloodRequest request, Donor user)
    {
        if (request.AuthorId == user.Id)
        {
            return (false, "Це ваш запит.");
        }

        if (user.BloodGroup is not { } group || user.RhFactor is not { } rh)
        {
            return (false, "Вкажіть групу крові й резус у профілі.");
        }

        if (!BloodCompatibility.CanDonate(group, rh, request.BloodGroup, request.RhFactor, request.Type))
        {
            return (false, $"Ваша група {ProfileValidator.FormatBloodType(group, rh)} не підходить.");
        }

        var eligibility = _calculator.Calculate(user.Sex, user.Donations, request.Type, Today);
        if (!eligibility.CanDonateNow)
        {
            return (false, $"Ви зможете здавати з {eligibility.NextDate:dd.MM.yyyy}.");
        }

        return (true, null);
    }

    public static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || !PhoneChars().IsMatch(phone))
        {
            return false;
        }

        var digits = phone.Count(char.IsDigit);
        return digits is >= 10 and <= 15;
    }

    [GeneratedRegex(@"^\+?[\d\s\-()]+$")]
    private static partial Regex PhoneChars();

    private IQueryable<BloodRequest> Query() =>
        _db.BloodRequests
            .AsNoTracking()
            .Include(r => r.BloodCenter)
            .Include(r => r.Responses).ThenInclude(x => x.Donor);

    private async Task<BloodRequest> FindAsync(int requestId, CancellationToken ct) =>
        await Query().FirstOrDefaultAsync(r => r.Id == requestId, ct)
        ?? throw ServiceException.NotFound("Запит не знайдено.");

    private async Task<Donor> LoadUserAsync(int userId, CancellationToken ct) =>
        await _db.Donors.AsNoTracking().Include(d => d.Donations).FirstOrDefaultAsync(d => d.Id == userId, ct)
        ?? throw ServiceException.NotFound("Користувача не знайдено.");

    private static double? Distance(BloodRequest r, double? lat, double? lng) =>
        lat is { } la && lng is { } lo && r.BloodCenter is { } c
            ? Math.Round(GeoDistance.Kilometers(la, lo, c.Latitude, c.Longitude), 2)
            : null;

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private BloodRequestDto ToDto(BloodRequest r, Donor user, double? distance)
    {
        var isMine = r.AuthorId == user.Id;
        var responded = r.Responses.Any(x => x.DonorId == user.Id);
        var (canHelp, reason) = r.Status == RequestStatus.Open
            ? CheckCanHelp(r, user)
            : (false, "Запит закритий.");

        var responders = isMine
            ? r.Responses
                .OrderBy(x => x.CreatedAt)
                .Select(x => new ResponderDto(
                    x.DonorId,
                    x.Donor?.FullName ?? "",
                    x.Donor?.Email ?? "",
                    ProfileValidator.FormatBloodType(x.Donor?.BloodGroup, x.Donor?.RhFactor),
                    x.CreatedAt))
                .ToList()
            : null;

        return new BloodRequestDto(
            r.Id,
            r.BloodGroup,
            r.RhFactor,
            ProfileValidator.FormatBloodType(r.BloodGroup, r.RhFactor),
            r.Type,
            r.UnitsNeeded,
            r.Urgency,
            r.Status,
            r.BloodCenterId,
            r.BloodCenter?.Name ?? "",
            r.BloodCenter?.Address ?? "",
            r.BloodCenter?.City ?? "",
            r.Hospital,
            r.PatientName,
            r.Description,
            r.NeededBy,
            r.CreatedAt,
            distance,
            r.Responses.Count,
            isMine,
            responded,
            canHelp,
            canHelp ? null : reason,
            isMine || responded ? r.ContactPhone : null,
            responders);
    }
}
