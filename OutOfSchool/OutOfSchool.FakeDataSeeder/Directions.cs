using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Directions
    {
        private readonly OutOfSchoolDbContext context;

        public Directions(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var directions = new List<Direction>()
            {
                new Direction()
                {
                    Id = 1,
                    Title = "Музика",
                    Description = "Музика",
                },
                new Direction()
                {
                    Id = 2,
                    Title = "Танці",
                    Description = "Танці",
                },
                new Direction()
                {
                    Id = 3,
                    Title = "Спорт",
                    Description = "Спорт",
                },
            };

            context.Directions.AddRange(directions);
        }
    }
}
