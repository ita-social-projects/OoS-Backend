using H3Lib;
using H3Lib.Extensions;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class GeoMathHelperExtensions
    {
        public static City AddGeoHash(this City city)
        {
            if (!(city is null))
            {
                city.GeoHash = Api.GeoToH3(default(GeoCoord).SetDegrees((decimal)city.Latitude, (decimal)city.Longitude), GeoMathHelper.ResolutionForCity);
            }

            return city;
        }
    }
}
