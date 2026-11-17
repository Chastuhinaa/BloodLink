using System.ComponentModel.DataAnnotations;
using BloodLink.Domain;

namespace BloodLink.Api.Dtos;

/// <summary>Новий запит на кров для пацієнта.</summary>
public record CreateBloodRequestRequest(
    [Required(ErrorMessage = "Вкажіть групу крові.")] BloodGroup BloodGroup,
    [Required(ErrorMessage = "Вкажіть резус.")] RhFactor RhFactor,
    DonationType? Type,
    [Range(1, 20, ErrorMessage = "Кількість донорів — від 1 до 20.")] int? UnitsNeeded,
    [Required(ErrorMessage = "Вкажіть центр крові.")] int BloodCenterId,
    RequestUrgency? Urgency,
    [Required(ErrorMessage = "Вкажіть телефон для зв'язку."), MaxLength(20)] string ContactPhone,
    [MaxLength(100)] string? PatientName,
    [MaxLength(200)] string? Hospital,
    [MaxLength(500)] string? Description,
    DateOnly? NeededBy);

/// <summary>Зміна статусу запиту автором.</summary>
public record UpdateRequestStatusRequest([Required] RequestStatus Status);

/// <summary>Донор, який відгукнувся на запит (бачить лише автор запиту).</summary>
public record ResponderDto(int DonorId, string FullName, string Email, string BloodType, DateTime RespondedAt);

/// <summary>
/// Запит на кров. CanHelp — чи може поточний користувач допомогти (сумісна група і минув інтервал);
/// ContactPhone видно лише автору і донорам, які відгукнулися; Responders — лише автору.
/// </summary>
public record BloodRequestDto(
    int Id,
    BloodGroup BloodGroup,
    RhFactor RhFactor,
    string BloodType,
    DonationType Type,
    int UnitsNeeded,
    RequestUrgency Urgency,
    RequestStatus Status,
    int BloodCenterId,
    string BloodCenterName,
    string BloodCenterAddress,
    string City,
    string? Hospital,
    string? PatientName,
    string? Description,
    DateOnly? NeededBy,
    DateTime CreatedAt,
    double? DistanceKm,
    int ResponsesCount,
    bool IsMine,
    bool IResponded,
    bool CanHelp,
    string? CannotHelpReason,
    string? ContactPhone,
    IReadOnlyList<ResponderDto>? Responders);
