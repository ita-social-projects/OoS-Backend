using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Classes
    {
        private readonly OutOfSchoolDbContext context;

        public Classes(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var classes = new List<Class>()
            {
                new Class()
                {
                    Title = "Бандура",
                    Description = "Клас Бандури",
                    DepartmentId = 1,
                },
                new Class()
                {
                    Title = "Акордеон",
                    Description = "Клас Акордеону",
                    DepartmentId = 1,
                },
                new Class()
                {
                    Title = "Ударні",
                    Description = "Клас Ударних",
                    DepartmentId = 2,
                },
                new Class()
                {
                    Title = "Флейта",
                    Description = "Клас Флейти",
                    DepartmentId = 2,
                },
                new Class()
                {
                    Title = "Бальні танці",
                    Description = "Клас Бального танцю",
                    DepartmentId = 3,
                },
                new Class()
                {
                    Title = "Сучасні танці",
                    Description = "Клас Сучасного танцю",
                    DepartmentId = 3,
                },
                new Class()
                {
                    Title = "Плавання",
                    Description = "I.030. Плавання",
                    DepartmentId = 4,
                },
                new Class()
                {
                    Title = "Футбол",
                    Description = "I.050. Футбол",
                    DepartmentId = 4,
                },
                new Class()
                {
                    Title = "Айкідо",
                    Description = "II.004. Айкідо",
                    DepartmentId = 5,
                },
                new Class()
                {
                    Title = "Альпінізм",
                    Description = "II.007. Альпінізм",
                    DepartmentId = 5,
                },
            };

            context.Classes.AddRange(classes);
        }
    }
}
