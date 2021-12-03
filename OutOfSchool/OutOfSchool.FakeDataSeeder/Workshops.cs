using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.FakeDataSeeder
{
    public class Workshops
    {
        private readonly OutOfSchoolDbContext context;

        public Workshops(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var workshops = new List<Workshop>()
            {
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Уроки аккордиону",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider1@test.com",
                    Website = "http://provider1",
                    Facebook = "http://facebook/provider1",
                    Instagram = "http://instagram/provider1",
                    MinAge = 5,
                    MaxAge = 100,
                    Price = 50,
                    Description = "Уроки аккордиону",
                    WithDisabilityOptions = true,
                    DisabilityOptionsDesc = "Немає конкретних обмежень",
                    Logo = "Logo",
                    Head = "Василенко Світлана Львівна",
                    HeadDateOfBirth = new DateTime(1987, 09, 22),
                    IsPerMonth = true,
                    ProviderId = new Guid("08d9b63b-2fbe-47c7-8280-48e72196c991"),
                    AddressId = 5,
                    DirectionId = 1,
                    DepartmentId = 2,
                    ClassId = 2,
                    ProviderTitle = "Музична школа №1",                    
                },
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Уроки бандури",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider1@test.com",
                    Website = "http://provider1",
                    Facebook = "http://facebook/provider1",
                    Instagram = "http://instagram/provider1",
                    MinAge = 5,
                    MaxAge = 100,
                    Price = 500,
                    Description = "Уроки бандури",
                    WithDisabilityOptions = true,
                    DisabilityOptionsDesc = "Немає конкретних обмежень",
                    Logo = "Logo",
                    Head = "Денисенко Денис Денисович",
                    HeadDateOfBirth = new DateTime(1987, 09, 22),
                    IsPerMonth = true,
                    ProviderId = new Guid("08d9b63b-2fbe-47c7-8280-48e72196c991"),
                    AddressId = 6,
                    DirectionId = 1,
                    DepartmentId = 1,
                    ClassId = 1,
                    ProviderTitle = "Музична школа №1",
                },
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Гра на барабані",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider1@test.com",
                    Website = "http://provider1",
                    Facebook = "http://facebook/provider1",
                    Instagram = "http://instagram/provider1",
                    MinAge = 5,
                    MaxAge = 100,
                    Price = 500,
                    Description = "Уроки гри на ударних інструментах",
                    WithDisabilityOptions = true,
                    DisabilityOptionsDesc = "Немає конкретних обмежень",
                    Logo = "Logo",
                    Head = "Гуляйборода Катерина Василівна",
                    HeadDateOfBirth = new DateTime(1977, 09, 22),
                    IsPerMonth = false,
                    ProviderId = new Guid("08d9b63b-2fbe-47c7-8280-48e72196c991"),
                    AddressId = 7,
                    DirectionId = 1,
                    DepartmentId = 2,
                    ClassId = 3,
                    ProviderTitle = "Музична школа №1",
                },
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Уроки гри на флейті",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider1@test.com",
                    Website = "http://provider1",
                    Facebook = "http://facebook/provider1",
                    Instagram = "http://instagram/provider1",
                    MinAge = 5,
                    MaxAge = 100,
                    Price = 100,
                    Description = "Уроки гри на флейті",
                    WithDisabilityOptions = true,
                    DisabilityOptionsDesc = "Немає конкретних обмежень",
                    Logo = "Logo",
                    Head = "Гуляйборода Катерина Василівна",
                    HeadDateOfBirth = new DateTime(1977, 09, 22),
                    IsPerMonth = false,
                    ProviderId = new Guid("08d9b63b-2fbe-47c7-8280-48e72196c991"),
                    AddressId = 8,
                    DirectionId = 1,
                    DepartmentId = 2,
                    ClassId = 4,
                    ProviderTitle = "Музична школа №1",
                },
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Айкідо",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider2@test.com",
                    Website = "http://provider2",
                    Facebook = "http://facebook/provider2",
                    Instagram = "http://instagram/provider2",
                    MinAge = 7,
                    MaxAge = 50,
                    Price = 300,
                    Description = "Уроки айкідо",
                    WithDisabilityOptions = false,
                    DisabilityOptionsDesc = null,
                    Logo = "Логотип",
                    Head = "Дорогий Захар Несторович",
                    HeadDateOfBirth = new DateTime(1984, 09, 02),
                    IsPerMonth = true,
                    ProviderId = new Guid("08d9b63b-2fcc-4c52-829d-c296d7323888"),
                    AddressId = 9,
                    DirectionId = 3,
                    DepartmentId = 5,
                    ClassId = 9,
                    ProviderTitle = "Школа бойових мистецтв №2",
                },
                new Workshop()
                {
                    //Id = new Guid(""),
                    Title = "Плавання",
                    Keywords = null,
                    Phone = "1234567890",
                    Email = "provider2@test.com",
                    Website = "http://provider2",
                    Facebook = "http://facebook/provider2",
                    Instagram = "http://instagram/provider2",
                    MinAge = 3,
                    MaxAge = 100,
                    Price = 300,
                    Description = "Уроки плавання",
                    WithDisabilityOptions = true,
                    DisabilityOptionsDesc = "будь-які",
                    Logo = "Логотип",
                    Head = "Рибочкін Леонід Федорович",
                    HeadDateOfBirth = new DateTime(1995, 09, 06),
                    IsPerMonth = true,
                    ProviderId = new Guid("08d9b63b-2fcc-4c52-829d-c296d7323888"),
                    AddressId = 10,
                    DirectionId = 3,
                    DepartmentId = 4,
                    ClassId = 7,
                    ProviderTitle = "Школа бойових мистецтв №2",
                },
            };

            context.Workshops.AddRange(workshops);
        }
    }
}
