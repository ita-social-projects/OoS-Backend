using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.FakeDataSeeder
{
    public class Providers
    {
        private readonly OutOfSchoolDbContext context;

        public Providers(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var providers = new List<Provider>()
            {
                new Provider()
                {
                    Id = new Guid("08d9b63b-2fbe-47c7-8280-48e72196c991"),
                    FullTitle = "Музична школа №1",
                    ShortTitle = "Музична школа",
                    Website = "http://provider1",
                    Email = "provider1@test.com",
                    Facebook = "http://facebook/provider1",
                    Instagram = "http://instagram/provider1",
                    Description = "Музикальні гуртки",
                    EdrpouIpn = 12345678,
                    Director = "Провайдерперший Семен Семенович",
                    DirectorDateOfBirth = new DateTime(2000, 10, 12),
                    PhoneNumber = "0981234567",
                    Founder = "Ващенко Володимир Богданович",
                    Ownership = OwnershipType.State,
                    Type = ProviderType.EducationalInstitution,
                    Status = true,
                    LegalAddressId = 1,
                    ActualAddressId = 2,
                    UserId = "47802b21-2fb5-435e-9057-75c43d002cef",
                },
                new Provider()
                {
                    Id = new Guid("08d9b63b-2fcc-4c52-829d-c296d7323888"),
                    FullTitle = "Школа бойових мистецтв №2",
                    ShortTitle = "ШБК №2",
                    Website = "http://provider2",
                    Email = "provider2@test.com",
                    Facebook = "http://facebook/provider2",
                    Instagram = "http://instagram/provider2",
                    Description = "Спортивні гуртки",
                    EdrpouIpn = 98764523,
                    Director = "Дорогий Захар Несторович",
                    DirectorDateOfBirth = new DateTime(1990, 11, 02),
                    PhoneNumber = "0981234567",
                    Founder = "Дорогий Захар Несторович",
                    Ownership = OwnershipType.Private,
                    Type = ProviderType.Private,
                    Status = true,
                    LegalAddressId = 3,
                    ActualAddressId = 4,
                    UserId = "5bff5f95-1848-4c87-9846-a567aeb407ea",
                },
            };

            context.Providers.AddRange(providers);
        }
    }
}
