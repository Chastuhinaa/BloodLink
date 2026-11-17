using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class DonationIntervalCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 15);
    private readonly DonationIntervalCalculator _calc = new();

    private static Donation D(int daysAgo, DonationType type = DonationType.WholeBlood) =>
        new() { Date = Today.AddDays(-daysAgo), Type = type };

    [Fact]
    public void NoHistory_CanDonateToday()
    {
        var r = _calc.Calculate(Sex.Female, Array.Empty<Donation>(), DonationType.WholeBlood, Today);

        Assert.True(r.CanDonateNow);
        Assert.Equal(Today, r.NextDate);
        Assert.Equal(0, r.DaysLeft);
    }

    [Fact]
    public void Male_WholeBlood_Waits60Days()
    {
        var r = _calc.Calculate(Sex.Male, new[] { D(10) }, DonationType.WholeBlood, Today);

        Assert.False(r.CanDonateNow);
        Assert.Equal(50, r.DaysLeft);
        Assert.Equal(Today.AddDays(50), r.NextDate);
    }

    [Fact]
    public void Female_WholeBlood_Waits90Days()
    {
        var r = _calc.Calculate(Sex.Female, new[] { D(10) }, DonationType.WholeBlood, Today);

        Assert.Equal(80, r.DaysLeft);
    }

    [Fact]
    public void UnknownSex_UsesStricterInterval()
    {
        var r = _calc.Calculate(null, new[] { D(70) }, DonationType.WholeBlood, Today);

        Assert.False(r.CanDonateNow);
        Assert.Equal(20, r.DaysLeft);
    }

    [Fact]
    public void ExactlyOnIntervalBoundary_CanDonate()
    {
        var r = _calc.Calculate(Sex.Male, new[] { D(60) }, DonationType.WholeBlood, Today);

        Assert.True(r.CanDonateNow);
    }

    [Fact]
    public void Plasma_After_Plasma_Waits14Days()
    {
        var r = _calc.Calculate(Sex.Female, new[] { D(4, DonationType.Plasma) }, DonationType.Plasma, Today);

        Assert.Equal(10, r.DaysLeft);
    }

    [Fact]
    public void UsesLatestDonation_RegardlessOfOrder()
    {
        var history = new[] { D(200), D(5, DonationType.Plasma), D(100) };

        var r = _calc.Calculate(Sex.Male, history, DonationType.WholeBlood, Today);

        Assert.Equal(9, r.DaysLeft);
    }

    [Fact]
    public void YearlyLimit_Male_BlocksSixthWholeBloodDonation()
    {
        var history = Enumerable.Range(1, 5).Select(i => D(61 * i)).ToList();

        var r = _calc.Calculate(Sex.Male, history, DonationType.WholeBlood, Today);

        Assert.False(r.CanDonateNow);
        Assert.Equal(60, r.DaysLeft);
        Assert.Contains("ліміт", r.Reason);
    }

    [Fact]
    public void FutureDonations_AreIgnored()
    {
        var future = new Donation { Date = Today.AddDays(3), Type = DonationType.WholeBlood };

        var r = _calc.Calculate(Sex.Male, new[] { future }, DonationType.WholeBlood, Today);

        Assert.True(r.CanDonateNow);
    }
}
