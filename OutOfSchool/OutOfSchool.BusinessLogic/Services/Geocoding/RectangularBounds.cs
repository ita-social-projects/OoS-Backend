namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Container for well known text representation of square bound around a point.
/// </summary>
public class RectangularBounds
{
    private const double EarthRadius = 6378.137;

    private const double Coef = (1 / ((2 * Math.PI / 360) * EarthRadius)) / 1000;

    private readonly Point northWest;

    private readonly Point northEast;

    private readonly Point southEast;

    private readonly Point southWest;

    public RectangularBounds(double lat, double lon, double deltaMeters)
    {
        northWest = CalculateNorthWest(lat, lon, deltaMeters);
        northEast = CalculateNorthEast(lat, lon, deltaMeters);
        southEast = CalculateSouthEast(lat, lon, deltaMeters);
        southWest = CalculateSouthWest(lat, lon, deltaMeters);
    }

    // Creates a well known text representation of a geometric polygon
    public string WKT =>
        $"POLYGON (( {northWest}, {northEast}, {southEast}, {southWest}, {northWest} ))";

    private static Point CalculateSouthWest(double lat, double lon, double deltaMeters)
    {
        return new Point(lat - (deltaMeters * Coef), lon - (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private static Point CalculateNorthWest(double lat, double lon, double deltaMeters)
    {
        return new Point(lat + (deltaMeters * Coef), lon - (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private static Point CalculateNorthEast(double lat, double lon, double deltaMeters)
    {
        return new Point(lat + (deltaMeters * Coef), lon + (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private static Point CalculateSouthEast(double lat, double lon, double deltaMeters)
    {
        return new Point(lat - (deltaMeters * Coef), lon + (deltaMeters * Coef / Math.Cos(GeoMathHelper.Deg2Rad(lat))));
    }

    private sealed record Point(double lat, double lon)
    {
        public override string ToString()
        {
            IFormatProvider dotSeparatorFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
            return $"{lon.ToString(dotSeparatorFormat)} {lat.ToString(dotSeparatorFormat)}";
        }
    }
}