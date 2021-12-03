using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.FakeDataSeeder
{
    public class Children
    {
        private readonly OutOfSchoolDbContext context;

        public Children(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var children = new List<Child>()
            {
                new Child()
                {
                    FirstName = "Тетяна",
                    LastName = "Батькоперший",
                    MiddleName = "Іванівна",
                    DateOfBirth = new DateTime(2010,12,11),
                    Gender = Gender.Female,
                    ParentId = new Guid("08d9b633-7deb-4d4c-8c12-c25de0fed4e9"),
                    SocialGroupId = null,
                    PlaceOfStudy = "Загальноосвітня школа №125",
                },
                new Child()
                {
                    FirstName = "Богдан",
                    LastName = "Батькодругий",
                    MiddleName = "Петрович",
                    DateOfBirth = new DateTime(2010,05,05),
                    Gender = Gender.Male,
                    ParentId = new Guid("08d9b633-7df4-4651-8c6c-7ffd08d4c3a0"),
                    SocialGroupId = 2,
                    PlaceOfStudy = "ЗО №14",
                },
                new Child()
                {
                    FirstName = "Лідія",
                    LastName = "Батькодругий",
                    MiddleName = "Петрівна",
                    DateOfBirth = new DateTime(2010,10,01),
                    Gender = Gender.Female,
                    ParentId = new Guid("08d9b633-7df4-4651-8c6c-7ffd08d4c3a0"),
                    SocialGroupId = 2,
                    PlaceOfStudy = "СШ №1",
                },
            };

            context.Children.AddRange(children);
        }
    }
}
