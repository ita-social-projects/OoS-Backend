using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;
using OutOfSchool.Services;
using System.Linq;
using OutOfSchool.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.FakeDataSeeder
{
    public class InitialPredefinedData
    {
        public static void Create()
        {
            var connectionString = "";
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            var options = new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseMySql(connectionString, serverVersion)
                .Options;

            var users = Users.GetPredefined();
            
            using (var context = new OutOfSchoolDbContext(options))
            {
                //context.Users.AddRange(users);

                //var roles = new Roles(context);
                //roles.Create();

                //new Parents(context).Create();

                //new Children(context).Create();

                //new Directions(context).Create();
                //new Departments(context).Create();
                //new Classes(context).Create();

                //new Addresses(context).Create();

                //new Providers(context).Create();

                new Workshops(context).Create();

                context.SaveChanges();

            }            
        }
    }
}
