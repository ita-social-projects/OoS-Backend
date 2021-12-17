using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Tests.Common.TestDataGenerators
{
    public class PermissionsForRolesGenerator
    {
        private static Faker<PermissionsForRole> faker = new Faker<PermissionsForRole>()
            .RuleFor(x => x.PackedPermissions, _ => TestDataHelper.GetFakePackedPermissions())
            .RuleFor(x => x.Description, _ => TestDataHelper.GetRandomWords());

        /// <summary>
        /// Generates new instance of the <see cref="PermissionsForRole"/> class.
        /// </summary>
        /// <returns><see cref="PermissionsForRole"/> object with random data for one of the existing roles.</returns>
        public static PermissionsForRole Generate() => faker
            .RuleFor(x => x.RoleName, _ => TestDataHelper.GetRandomRole())
            .Generate();

        /// <summary>
        /// Generates a list of the <see cref="PermissionsForRole"/> objects with random role names.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="PermissionsForRole"/> objects.</returns>
        public static List<PermissionsForRole> Generate(int count) => faker.Generate(count);


        /// <summary>
        /// Generates new instance of the <see cref="PermissionsForRole"/> class for predifined role.
        /// </summary>
        /// <returns><see cref="PermissionsForRole"/> object with random data.</returns>
        public static PermissionsForRole Generate(string roleName) => faker
            .RuleFor(x => x.RoleName, _ => roleName);

        /// <summary>
        /// Generates a list of the <see cref="PermissionsForRole"/> objects for existing roles.
        /// </summary>
        /// <param name="count">count of instances to generate.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="PermissionsForRole"/> objects.</returns>
        public static List<PermissionsForRole> GenerateForExistingRoles()
        {
            var list = new List<PermissionsForRole>();
            var roles = (Role[])Enum.GetValues(typeof(Role));
            foreach (var role in roles)
            {
                list.Add(Generate(role.ToString()));
            }
            return list;
        }
    }
}

