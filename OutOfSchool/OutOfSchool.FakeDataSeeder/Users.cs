using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.FakeDataSeeder
{
    public class Users
    {
        public static List<User> GetPredefined()
        {
            var users = new List<User>()
            {
                new User()
                {
                    Id = "16575ce5-38e3-4ae7-b991-4508ed488369",
                    CreatingTime = DateTimeOffset.UtcNow,
                    LastLogin = default,
                    LastName = "Батькоперший",
                    MiddleName = "Іванович",
                    FirstName = "Іван",
                    Role = "parent",
                    UserName = "test1@test.com",
                    NormalizedUserName = "TEST1@TEST.COM",
                    Email = "test1@test.com",
                    NormalizedEmail = "TEST1@TEST.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAELVU2FZw3HwShwuAXJR/xKFl938KgGpwdRRegrC5UFgZ5gnXdV6mEfalZCAngmX5sQ==",
                    SecurityStamp = "5Z5EXGBVHXUYVKZOBCCG7QLPWI6NJ22O",
                    ConcurrencyStamp = "81dfc15c-1f36-48f6-99fd-9d028096cdec",
                    PhoneNumber = "1234567890",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnd = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsRegistered = true,
                },
                new User()
                {
                    Id = "7604a851-66db-4236-9271-1f037ffe3a81",
                    CreatingTime = DateTimeOffset.UtcNow,
                    LastLogin = default,
                    LastName = "Батькодругий",
                    MiddleName = "Петрович",
                    FirstName = "Петро",
                    Role = "parent",
                    UserName = "test2@test.com",
                    NormalizedUserName = "TEST2@TEST.COM",
                    Email = "test2@test.com",
                    NormalizedEmail = "TEST2@TEST.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEE6AlX8whARS9uZwJ5AUZx8490dgEnfrv7Q1lBXFqJwZcSoN6Mnvadhg75HG3ooT/A==",
                    SecurityStamp = "PB7OZCNE7PMY4YDB3VE34U5K2TXWGTLP",
                    ConcurrencyStamp = "83915185-5bbd-4047-9901-638de8bd3a27",
                    PhoneNumber = "4561237890",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnd = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsRegistered = true,
                },
                new User()
                {
                    Id = "47802b21-2fb5-435e-9057-75c43d002cef",
                    CreatingTime = DateTimeOffset.UtcNow,
                    LastLogin = default,
                    LastName = "Провайдерперший",
                    MiddleName = "Семенович",
                    FirstName = "Семен",
                    Role = "provider",
                    UserName = "test3@test.com",
                    NormalizedUserName = "TEST3@TEST.COM",
                    Email = "test3@test.com",
                    NormalizedEmail = "TEST3@TEST.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAELY6XF2g82E4EWQJl6UFWlojctlsLegV4f6qoME2IwwdI5fGaOq/Y6L6t+oa1N9j4Q==",
                    SecurityStamp = "EC36E7KFYXF2YMCSOSD27UXQRFKPMDKK",
                    ConcurrencyStamp = "789c5891-212c-4339-95a2-80f75a168231",
                    PhoneNumber = "7890123456",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnd = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsRegistered = true,
                },
                new User()
                {
                    Id = "5bff5f95-1848-4c87-9846-a567aeb407ea",
                    CreatingTime = DateTimeOffset.UtcNow,
                    LastLogin = default,
                    LastName = "Провайдердругий",
                    MiddleName = "Борисович",
                    FirstName = "Борис",
                    Role = "provider",
                    UserName = "test4@test.com",
                    NormalizedUserName = "TEST4@TEST.COM",
                    Email = "test4@test.com",
                    NormalizedEmail = "TEST4@TEST.COM",
                    EmailConfirmed = false,
                    PasswordHash = "AQAAAAEAACcQAAAAEOJFUEjnmnHWPonAOsg9K6tBuT8e1cUYbBejJbRJf3smSTzUzqphZyFGtB7i6vuT0g==",
                    SecurityStamp = "NQTDJHP23OUXWSVOUSBDSUVBASPLCHV3",
                    ConcurrencyStamp = "b419d5b7-fe0f-40a8-869f-76b2826de58f",
                    PhoneNumber = "0123456789",
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnd = null,
                    LockoutEnabled = true,
                    AccessFailedCount = 0,
                    IsRegistered = true,
                }
            };

            return users;
        }
    }
}
