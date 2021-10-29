using System;

namespace OutOfSchool.Common
{
    public static class GeoMathHelper
    {
        public static decimal GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3; // Radius of the earth in m
            var dLat = Deg2rad(lat2 - lat1);  // Deg2rad below
            var dLon = Deg2rad(lon2 - lon1);
            var a =
                    (Math.Sin(dLat / 2) * Math.Sin(dLat / 2)) +
                    (Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2));
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = r * c; // Distance in km
            return (decimal)d;
        }

        public static double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}