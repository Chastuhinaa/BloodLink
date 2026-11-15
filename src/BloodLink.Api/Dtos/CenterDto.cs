namespace BloodLink.Api.Dtos;

/// <summary>Центр крові з відстанню до донора.</summary>
public record CenterDto(
    int Id,
    string Name,
    string Address,
    string City,
    double Latitude,
    double Longitude,
    string? Phone,
    string? WorkingHours,
    double? DistanceKm);
