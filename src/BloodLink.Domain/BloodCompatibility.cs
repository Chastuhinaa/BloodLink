namespace BloodLink.Domain;

// Спрощені правила сумісності (AB0 + резус). Остаточне рішення завжди за центром крові.
public static class BloodCompatibility
{
    public static bool CanDonate(
        BloodGroup donorGroup, RhFactor donorRh,
        BloodGroup recipientGroup, RhFactor recipientRh,
        DonationType type)
    {
        if (type == DonationType.WholeBlood)
        {
            var aboOk = (Antigens(donorGroup) & ~Antigens(recipientGroup)) == 0;
            var rhOk = donorRh == RhFactor.Negative || recipientRh == RhFactor.Positive;
            return aboOk && rhOk;
        }

        return (Antigens(recipientGroup) & ~Antigens(donorGroup)) == 0;
    }

    public static IReadOnlyList<(BloodGroup Group, RhFactor Rh)> CompatibleDonors(
        BloodGroup recipientGroup, RhFactor recipientRh, DonationType type)
    {
        var result = new List<(BloodGroup, RhFactor)>();
        foreach (var g in Enum.GetValues<BloodGroup>())
        {
            foreach (var rh in Enum.GetValues<RhFactor>())
            {
                if (CanDonate(g, rh, recipientGroup, recipientRh, type))
                {
                    result.Add((g, rh));
                }
            }
        }

        return result;
    }

    private static int Antigens(BloodGroup group) => group switch
    {
        BloodGroup.O => 0,
        BloodGroup.A => 1,
        BloodGroup.B => 2,
        BloodGroup.AB => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(group))
    };
}
