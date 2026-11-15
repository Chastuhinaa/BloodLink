using System.ComponentModel.DataAnnotations;
using BloodLink.Domain;

namespace BloodLink.Api.Dtos;

/// <summary>Профіль донора.</summary>
public record ProfileDto(
    int Id,
    string Email,
    string FullName,
    Sex? Sex,
    DateOnly? BirthDate,
    BloodGroup? BloodGroup,
    RhFactor? RhFactor,
    string BloodType,
    string? City,
    double? Latitude,
    double? Longitude);

/// <summary>Оновлення профілю донора.</summary>
public record UpdateProfileRequest(
    [Required(ErrorMessage = "Вкажіть ім'я."), MaxLength(100)] string FullName,
    Sex? Sex,
    DateOnly? BirthDate,
    BloodGroup? BloodGroup,
    RhFactor? RhFactor,
    [MaxLength(100)] string? City,
    double? Latitude,
    double? Longitude);
