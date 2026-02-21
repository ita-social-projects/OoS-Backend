using Elastic.Clients.Elasticsearch;

namespace OutOfSchool.BusinessLogic.Extensions;

public static class GeoLocationExtensions
{
    public static double? GetLatitude(this GeoLocation geoLocation)
    {
        if (geoLocation != null && geoLocation.TryGetLatitudeLongitude(out var latLonGeoLocation))
        {
            return latLonGeoLocation.Lat;
        }

        return null;
    }

    public static double? GetLongitude(this GeoLocation geoLocation)
    {
        if (geoLocation != null && geoLocation.TryGetLatitudeLongitude(out var latLonGeoLocation))
        {
            return latLonGeoLocation.Lon;
        }

        return null;
    }
}
