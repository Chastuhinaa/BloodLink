using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class BloodCompatibilityTests
{
    [Theory]
    [InlineData(BloodGroup.O, RhFactor.Negative, BloodGroup.AB, RhFactor.Positive, true)]
    [InlineData(BloodGroup.O, RhFactor.Negative, BloodGroup.A, RhFactor.Negative, true)]
    [InlineData(BloodGroup.A, RhFactor.Positive, BloodGroup.A, RhFactor.Negative, false)]
    [InlineData(BloodGroup.A, RhFactor.Negative, BloodGroup.B, RhFactor.Negative, false)]
    [InlineData(BloodGroup.AB, RhFactor.Positive, BloodGroup.O, RhFactor.Positive, false)]
    [InlineData(BloodGroup.B, RhFactor.Positive, BloodGroup.AB, RhFactor.Positive, true)]
    public void WholeBlood_FollowsAboAndRhRules(BloodGroup dg, RhFactor drh, BloodGroup rg, RhFactor rrh, bool expected)
    {
        Assert.Equal(expected, BloodCompatibility.CanDonate(dg, drh, rg, rrh, DonationType.WholeBlood));
    }

    [Fact]
    public void Plasma_AbIsUniversalDonor_AndRhIgnored()
    {
        foreach (var g in Enum.GetValues<BloodGroup>())
        {
            Assert.True(BloodCompatibility.CanDonate(BloodGroup.AB, RhFactor.Positive, g, RhFactor.Negative, DonationType.Plasma));
        }

        Assert.False(BloodCompatibility.CanDonate(BloodGroup.O, RhFactor.Negative, BloodGroup.A, RhFactor.Positive, DonationType.Plasma));
    }

    [Fact]
    public void CompatibleDonors_ForONegative_OnlyONegative()
    {
        var donors = BloodCompatibility.CompatibleDonors(BloodGroup.O, RhFactor.Negative, DonationType.WholeBlood);

        Assert.Equal(new[] { (BloodGroup.O, RhFactor.Negative) }, donors);
    }

    [Fact]
    public void CompatibleDonors_ForAbPositive_AllEightTypes()
    {
        Assert.Equal(8, BloodCompatibility.CompatibleDonors(BloodGroup.AB, RhFactor.Positive, DonationType.WholeBlood).Count);
    }
}
