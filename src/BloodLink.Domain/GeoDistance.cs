namespace BloodLink.Domain;

public static class GeoDistance
{
    private const double EarthRadiusKm = 6371.0;

    public static double Kilometers(double lat1, double lon1, double lat2, double lon2)
    {
        ValidateCoordinates(lat1, lon1);
        ValidateCoordinates(lat2, lon2);

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    public static void ValidateCoordinates(double lat, double lon)
    {
        if (lat is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(lat), lat, "Широта має бути в межах [-90; 90].");
        }

        if (lon is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(lon), lon, "Довгота має бути в межах [-180; 180].");
        }
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
