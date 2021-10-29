using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H3Lib;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.WebApi.Services
{
    public class H3GeoService
    {
        private readonly IEntityRepository<Address> addressRepository;
        private readonly IWorkshopRepository workshopRepository;

        public H3GeoService(IEntityRepository<Address> addressRepository, IWorkshopRepository workshopRepository)
        {
            this.addressRepository = addressRepository;
            this.workshopRepository = workshopRepository;
        }

        public async Task<int> GenerateGeoHash()
        {
            foreach (var address in addressRepository.Get<int>())
            {
                var newie = address;
                var geo = new H3Lib.GeoCoord((decimal)newie.Latitude, (decimal)newie.Longitude);
                newie.GeoHash = H3Lib.Api.GeoToH3(geo, 6).Value;
                await addressRepository.Update(newie).ConfigureAwait(false);
            }

            return 0;
        }

        public async Task<List<(long, decimal)>> FindNearestWorkshopsId(double lat, double lot, int count)
        {
            int kRing = 1;
            var geo = new GeoCoord((decimal)lat, (decimal)lot);
            var h3Location = H3Lib.Api.GeoToH3(geo, 6);
            var workshops = workshopRepository.Get<int>();
            Api.KRing(h3Location, kRing, out var neighbours);
            neighbours.Add(h3Location);

            // var neighboursMapped = neighbours.Select(n => n.Value);

            var closestWorkshops = workshops
                .Include(w => w.Address)
                .Where(w => neighbours
                    .Select(n => n.Value)
                    .Any(hash => hash == w.Address.GeoHash));

            while (closestWorkshops.Count() < count && kRing < 10)
            {
                Api.KRing(h3Location, ++kRing, out neighbours);
                neighbours.Add(h3Location);

                closestWorkshops = workshops
                    .Include(w => w.Address)
                    .Where(w => neighbours
                        .Select(n => n.Value)
                        .Any(hash => hash == w.Address.GeoHash));
            }

            var enumerableWorkshops = closestWorkshops.AsEnumerable();

            var nearestWorkshopsIds = enumerableWorkshops
                .Select(w => new {Id = w.Id, Distance = GetDistanceFromLatLonInKm(w.Address.Latitude, w.Address.Longitude, (double)geo.Latitude, (double)geo.Longitude) })
                .OrderBy(p => p.Distance).Take(count).Select(a => (a.Id, a.Distance));
            return nearestWorkshopsIds.ToList();
        }

        public decimal GetDistanceFromLatLonInKm(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371e3; // Radius of the earth in m
            var dLat = deg2rad(lat2-lat1);  // deg2rad below
            var dLon = deg2rad(lon2-lon1);
            var a =
                    Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                    Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * 
                    Math.Sin(dLon/2) * Math.Sin(dLon/2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); 
            var d = R * c; // Distance in km
            return (decimal)d;
        }

        public double deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}