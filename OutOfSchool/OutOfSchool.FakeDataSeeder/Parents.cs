using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Parents
    {
        private readonly OutOfSchoolDbContext context;

        public Parents(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var parents = new List<Parent>()
            {
                new Parent()
                {
                    Id = new Guid("08d9b633-7deb-4d4c-8c12-c25de0fed4e9"),
                    UserId = "16575ce5-38e3-4ae7-b991-4508ed488369",
                },
                new Parent()
                {
                    Id = new Guid("08d9b633-7df4-4651-8c6c-7ffd08d4c3a0"),
                    UserId = "7604a851-66db-4236-9271-1f037ffe3a81",
                },
            };

            context.Parents.AddRange(parents);
        }
    }
}
