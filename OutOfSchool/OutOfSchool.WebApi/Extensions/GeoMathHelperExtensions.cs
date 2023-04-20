using H3Lib;
using H3Lib.Extensions;

namespace OutOfSchool.WebApi.Extensions;

public static class GeoMathHelperExtensions
{
    public static CATOTTG AddGeoHash(this CATOTTG city)
    {
        if (city is not null)
        {
            city.GeoHash = Api.GeoToH3(default(GeoCoord).SetDegrees((decimal)city.Latitude, (decimal)city.Longitude), GeoMathHelper.ResolutionForCity);
        }

        return city;
    }
}