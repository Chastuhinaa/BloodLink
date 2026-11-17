using BloodLink.Domain;

namespace BloodLink.Api.Dtos;

/// <summary>Коли донор може здавати наступного разу.</summary>
public record EligibilityDto(
    DonationType Type,
    bool CanDonateNow,
    DateOnly NextDate,
    int DaysLeft,
    string Reason);
