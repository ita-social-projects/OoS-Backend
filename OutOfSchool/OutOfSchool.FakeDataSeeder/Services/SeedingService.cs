using System;
using OutOfSchool.Services;
using System.Linq;

namespace OutOfSchool.FakeDataSeeder.Services
{
    public class SeedingService : ISeedingService
    {
        private readonly OutOfSchoolDbContext context;

        public SeedingService(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void FillWithPredefinedData()
        {
            var users = Users.GetPredefined();

            using (context)
            {
                context.Users.AddRange(users);

                var roles = new Roles(context);
                roles.Create();

                new Parents(context).Create();

                new Children(context).Create();

                new Directions(context).Create();

                new Departments(context).Create();

                new Classes(context).Create();

                new Addresses(context).Create();

                new Providers(context).Create();

                new Workshops(context).Create();

                context.SaveChanges();
            }
        }
    }
}
