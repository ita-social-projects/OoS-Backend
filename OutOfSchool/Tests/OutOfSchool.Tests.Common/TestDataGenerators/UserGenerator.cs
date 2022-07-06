using Bogus;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OutOfSchool.Tests.Common.TestDataGenerators;

/// <summary>
/// Contains methods to generate fake <see cref="User"/> objects.
/// </summary>
public static class UserGenerator
{
    private static readonly string PhoneFormat = "##########";

    private static readonly Faker<User> faker = new Faker<User>()
            .RuleFor(x => x.Id, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatingTime, f => f.Date.PastOffset())
            .RuleFor(x => x.LastLogin, f => f.Date.PastOffset())
            .RuleFor(x => x.MiddleName, f => f.Name.FirstName())
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.EmailConfirmed, _ => false)
            .RuleFor(x => x.PasswordHash, f => f.Random.Hash())
            .RuleFor(x => x.SecurityStamp,f => f.Random.Hash())
            .RuleFor(x => x.ConcurrencyStamp, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber(PhoneFormat))
            .RuleFor(x => x.Role, f => f.Random.ArrayElement((Role[])Enum.GetValues(typeof(Role))).ToString())
            .RuleFor(x => x.PhoneNumberConfirmed, _ => false)
            .RuleFor(x => x.TwoFactorEnabled, _ => false)
            .RuleFor(x => x.LockoutEnabled, _ => true)
            .RuleFor(x => x.AccessFailedCount, _ => 0)
        ;

    /// <summary>
    /// Creates new instance of the <see cref="User"/> class with random data.
    /// </summary>
    /// <returns><see cref="User"/> object.</returns>
    public static User Generate()
    {
        var email = TestDataHelper.GetRandomEmail();
        var normalisedEmail = email.ToUpper();
        return faker
            .RuleFor(x => x.UserName, _ => email)
            .RuleFor(x => x.Email, _ => email)
            .RuleFor(x => x.NormalizedUserName,_ => normalisedEmail)
            .RuleFor(x => x.NormalizedEmail,_ => normalisedEmail)
            .Generate();
    }

    /// <summary>
    /// Generates a list of the <see cref="User"/> objects.
    /// </summary>
    /// <param name="count">count of instances to generate.</param>
    public static List<User> Generate(int count) => faker.Generate(count);
}