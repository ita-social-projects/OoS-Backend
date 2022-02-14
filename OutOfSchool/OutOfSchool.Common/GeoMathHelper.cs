using System;

namespace OutOfSchool.Common
{
    public static class GeoMathHelper
    {
        public const int Resolution = 6;

        public const int ResolutionForCity = 5;

        public const int KRingForResolution = 1;

        public const string ElasticRadius = "5000m";

        public static decimal GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3; // Radius of the earth in m
            var dLat = Deg2Rad(lat2 - lat1); // Deg2rad below
            var dLon = Deg2Rad(lon2 - lon1);
            var a =
                (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                (Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) *
                 Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = r * c; // Distance in km
            return (decimal)d;
        }

        public static double Deg2Rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}