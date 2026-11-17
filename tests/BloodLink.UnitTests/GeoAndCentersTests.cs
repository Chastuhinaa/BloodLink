using BloodLink.Api.Data;
using BloodLink.Api.Services;
using BloodLink.Domain;

namespace BloodLink.UnitTests;

public class GeoAndCentersTests
{
    [Fact]
    public void Distance_SamePoint_IsZero()
    {
        Assert.Equal(0, GeoDistance.Kilometers(50.45, 30.52, 50.45, 30.52), 6);
    }

    [Fact]
    public void Distance_KyivToLviv_IsAbout470Km()
    {
        var km = GeoDistance.Kilometers(50.4501, 30.5234, 49.8397, 24.0297);

        Assert.InRange(km, 460, 480);
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var ab = GeoDistance.Kilometers(50.46, 30.51, 50.39, 30.51);
        var ba = GeoDistance.Kilometers(50.39, 30.51, 50.46, 30.51);

        Assert.Equal(ab, ba, 9);
    }

    [Theory]
    [InlineData(91, 30)]
    [InlineData(50, 181)]
    public void Distance_InvalidCoordinates_Throws(double lat, double lon)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeoDistance.Kilometers(lat, lon, 50, 30));
    }

    [Fact]
    public void SortByDistance_NearestFirst()
    {
        var result = CenterService.SortByDistance(SeedData.Centers, 50.5100, 30.4990);

        Assert.Equal(5, result[0].Id);
        Assert.True(result.Zip(result.Skip(1)).All(p => p.First.DistanceKm <= p.Second.DistanceKm));
    }

    [Fact]
    public void SortByDistance_WithoutCoordinates_SortsByNameWithoutDistance()
    {
        var result = CenterService.SortByDistance(SeedData.Centers, null, null);

        Assert.All(result, c => Assert.Null(c.DistanceKm));
        Assert.Equal(result.Select(c => c.Name).OrderBy(n => n), result.Select(c => c.Name));
    }

    [Fact]
    public void SortByDistance_RespectsLimit()
    {
        var result = CenterService.SortByDistance(SeedData.Centers, 50.45, 30.52, limit: 2);

        Assert.Equal(2, result.Count);
    }
}
