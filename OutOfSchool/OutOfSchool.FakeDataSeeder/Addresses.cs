using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Addresses
    {
        private readonly OutOfSchoolDbContext context;

        public Addresses(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var addresses = new List<Address>()
            {
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "29",
                    Latitude = 50.4547,
                    Longitude = 30.5238,                    
                },
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "35",
                    Latitude = 50.4547,
                    Longitude = 30.5238,
                },
                new Address()
                {
                    Region = "Житомирська обл.",
                    District = "м. Житомир",
                    City = "Житомир",
                    Street = "Вокзальна",
                    BuildingNumber = "10",
                    Latitude = 50.2648700,
                    Longitude = 28.6766900,
                },
                new Address()
                {
                    Region = "Житомирська обл.",
                    District = "м. Житомир",
                    City = "Житомир",
                    Street = "Привозна",
                    BuildingNumber = "12А",
                    Latitude = 50.2648700,
                    Longitude = 28.6766900,
                },
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "29",
                    Latitude = 50.4547,
                    Longitude = 30.5238,
                },
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "29",
                    Latitude = 50.4547,
                    Longitude = 30.5238,
                },
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "29",
                    Latitude = 50.4547,
                    Longitude = 30.5238,
                },
                new Address()
                {
                    Region = "Київська обл.",
                    District = "м. Київ",
                    City = "Київ",
                    Street = "Старонаводницька",
                    BuildingNumber = "29",
                    Latitude = 50.4547,
                    Longitude = 30.5238,
                },
                new Address()
                {
                    Region = "Житомирська обл.",
                    District = "м. Житомир",
                    City = "Житомир",
                    Street = "Привозна",
                    BuildingNumber = "12А",
                    Latitude = 50.2648700,
                    Longitude = 28.6766900,
                },
                new Address()
                {
                    Region = "Житомирська обл.",
                    District = "м. Житомир",
                    City = "Житомир",
                    Street = "Привозна",
                    BuildingNumber = "12А",
                    Latitude = 50.2648700,
                    Longitude = 28.6766900,
                },
            };

            context.Addresses.AddRange(addresses);
        }
    }
}
