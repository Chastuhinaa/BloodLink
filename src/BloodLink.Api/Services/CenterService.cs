using BloodLink.Api.Data;
using BloodLink.Api.Dtos;
using BloodLink.Domain;
using Microsoft.EntityFrameworkCore;

namespace BloodLink.Api.Services;

public interface ICenterService
{
    Task<IReadOnlyList<CenterDto>> GetAsync(double? latitude, double? longitude, int? limit, CancellationToken ct = default);
}

public class CenterService : ICenterService
{
    private readonly BloodLinkDbContext _db;

    public CenterService(BloodLinkDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CenterDto>> GetAsync(double? latitude, double? longitude, int? limit, CancellationToken ct = default)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw ServiceException.Validation(new[] { "Вкажіть і широту (lat), і довготу (lng)." });
        }

        if (limit is <= 0)
        {
            throw ServiceException.Validation(new[] { "Параметр limit має бути додатним." });
        }

        if (latitude is { } lat && longitude is { } lng)
        {
            try
            {
                GeoDistance.ValidateCoordinates(lat, lng);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw ServiceException.Validation(new[] { ex.Message.Split(" (Parameter")[0] });
            }
        }

        var centers = await _db.BloodCenters.AsNoTracking().ToListAsync(ct);
        return SortByDistance(centers, latitude, longitude, limit);
    }

    public static IReadOnlyList<CenterDto> SortByDistance(
        IEnumerable<BloodCenter> centers, double? latitude, double? longitude, int? limit = null)
    {
        IEnumerable<CenterDto> items;
        if (latitude is { } lat && longitude is { } lng)
        {
            items = centers
                .Select(c => ToDto(c, Math.Round(GeoDistance.Kilometers(lat, lng, c.Latitude, c.Longitude), 2)))
                .OrderBy(c => c.DistanceKm)
                .ThenBy(c => c.Name);
        }
        else
        {
            items = centers.Select(c => ToDto(c, null)).OrderBy(c => c.Name);
        }

        if (limit is { } n)
        {
            items = items.Take(n);
        }

        return items.ToList();
    }

    private static CenterDto ToDto(BloodCenter c, double? distance) =>
        new(c.Id, c.Name, c.Address, c.City, c.Latitude, c.Longitude, c.Phone, c.WorkingHours, distance);
}
