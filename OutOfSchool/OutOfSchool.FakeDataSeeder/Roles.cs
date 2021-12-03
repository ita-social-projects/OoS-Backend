using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;
using OutOfSchool.Services;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.FakeDataSeeder
{
    public class Roles
    {
        private readonly OutOfSchoolDbContext context;

        public Roles(OutOfSchoolDbContext context)
        {
            this.context = context;
        }

        public void Create()
        {
            var roles = context.Roles.Select(roles => roles).ToDictionary(name => name.Name, id => id.Id);

            var userRoles = new List<IdentityUserRole<string>>()
            {
                new IdentityUserRole<string>()
                {
                    UserId = "16575ce5-38e3-4ae7-b991-4508ed488369",
                    RoleId = roles["parent"],
                },
                new IdentityUserRole<string>()
                {
                    UserId = "7604a851-66db-4236-9271-1f037ffe3a81",
                    RoleId = roles["parent"],
                },
                new IdentityUserRole<string>()
                {
                    UserId = "47802b21-2fb5-435e-9057-75c43d002cef",
                    RoleId = roles["provider"],
                },
                new IdentityUserRole<string>()
                {
                    UserId = "5bff5f95-1848-4c87-9846-a567aeb407ea",
                    RoleId = roles["provider"],
                },
            };

            context.UserRoles.AddRange(userRoles);
        }
    }
}
