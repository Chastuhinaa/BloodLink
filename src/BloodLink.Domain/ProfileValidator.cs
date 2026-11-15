namespace BloodLink.Domain;

public static class ProfileValidator
{
    public const int MinDonorAge = 18;
    public const int MaxDonorAge = 65;

    public static IReadOnlyList<string> Validate(
        string? fullName,
        DateOnly? birthDate,
        double? latitude,
        double? longitude,
        DateOnly today)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors.Add("Вкажіть ім'я.");
        }
        else if (fullName.Trim().Length > 100)
        {
            errors.Add("Ім'я не може бути довшим за 100 символів.");
        }

        if (birthDate is { } bd)
        {
            var age = AgeOn(bd, today);
            if (bd > today)
            {
                errors.Add("Дата народження не може бути в майбутньому.");
            }
            else if (age < MinDonorAge)
            {
                errors.Add($"Донором можна стати з {MinDonorAge} років.");
            }
            else if (age > MaxDonorAge)
            {
                errors.Add($"Вік донора не може перевищувати {MaxDonorAge} років.");
            }
        }

        if (latitude.HasValue != longitude.HasValue)
        {
            errors.Add("Координати потрібно вказати разом: і широту, і довготу.");
        }
        else if (latitude is { } lat && longitude is { } lon)
        {
            if (lat is < -90 or > 90 || lon is < -180 or > 180)
            {
                errors.Add("Некоректні координати.");
            }
        }

        return errors;
    }

    public static int AgeOn(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        if (today < birthDate.AddYears(age))
        {
            age--;
        }

        return age;
    }

    public static string FormatBloodType(BloodGroup? group, RhFactor? rh)
    {
        if (group is null)
        {
            return "не вказано";
        }

        var name = group switch
        {
            BloodGroup.O => "0(I)",
            BloodGroup.A => "A(II)",
            BloodGroup.B => "B(III)",
            BloodGroup.AB => "AB(IV)",
            _ => "?"
        };

        return rh switch
        {
            RhFactor.Positive => $"{name} Rh+",
            RhFactor.Negative => $"{name} Rh−",
            _ => name
        };
    }
}
