using System.ComponentModel.DataAnnotations;
using BloodLink.Domain;

namespace BloodLink.Api.Dtos;

/// <summary>Запис в історії донацій.</summary>
public record DonationDto(
    int Id,
    DateOnly Date,
    DonationType Type,
    int? BloodCenterId,
    string? BloodCenterName,
    string? Notes);

/// <summary>Новий запис в історії донацій.</summary>
public record CreateDonationRequest(
    [Required] DateOnly Date,
    [Required] DonationType Type,
    int? BloodCenterId,
    [MaxLength(500)] string? Notes);
