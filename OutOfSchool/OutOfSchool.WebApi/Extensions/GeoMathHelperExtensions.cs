﻿using H3Lib;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public static class GeoMathHelperExtensions
    {
        public static City AddGeoHash(this City city)
        {
            city.GeoHash = Api.GeoToH3(new GeoCoord((decimal)city.Latitude, (decimal)city.Longitude), GeoMathHelper.ResolutionForCity);
            return city;
        }
    }
}
