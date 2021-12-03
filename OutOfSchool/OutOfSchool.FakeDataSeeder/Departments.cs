using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Departments
    {
        private readonly OutOfSchoolDbContext context;

        public Departments(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var departments = new List<Department>()
            {
                new Department()
                {
                    Id = 1,
                    Title = "Народних інструментів",
                    Description = "Народних інструментів",
                    DirectionId = 1,
                },
                new Department()
                {
                    Id = 2,
                    Title = "Духових та ударних інструментів",
                    Description = "Духових та ударних інструментів",
                    DirectionId = 1,
                },
                new Department()
                {
                    Id = 3,
                    Title = "Хореографічний",
                    Description = "Хореографічний",
                    DirectionId = 2,
                },
                new Department()
                {
                    Id = 4,
                    Title = "Олімпійські види спорту",
                    Description = "Олімпійські види спорту",
                    DirectionId = 3,
                },
                new Department()
                {
                    Id = 5,
                    Title = "Неолімпійські види спорту",
                    Description = "Неолімпійські види спорту",
                    DirectionId = 3,
                },
            };

            context.Departments.AddRange(departments);
        }
    }
}
