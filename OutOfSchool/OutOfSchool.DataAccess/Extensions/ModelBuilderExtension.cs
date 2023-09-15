using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Extensions;

public static class ModelBuilderExtension
{
    /// <summary>
    /// Add initial data for Social Group.
    /// </summary>
    /// <param name="builder">Model Builder.</param>
    public static void Seed(this ModelBuilder builder)
    {
        builder.Entity<SocialGroup>().HasData(
            new SocialGroup
            {
                Id = 1,
                Name = "Діти із багатодітних сімей",
                NameEn = "Children from large families",
            },
            new SocialGroup
            {
                Id = 2,
                Name = "Діти із малозабезпечених сімей",
                NameEn = "Children from low-income families",
            },
            new SocialGroup
            {
                Id = 3,
                Name = "Діти з інвалідністю",
                NameEn = "Children with disabilities",
            },
            new SocialGroup
            {
                Id = 4,
                Name = "Діти-сироти",
                NameEn = "Orphans",
            },
            new SocialGroup
            {
                Id = 5,
                Name = "Діти, позбавлені батьківського піклування",
                NameEn = "Children deprived of parental care",
            });

        builder.Entity<InstitutionStatus>().HasData(
            new InstitutionStatus
            {
                Id = 1,
                Name = "Працює",
                NameEn = "Works",
            },
            new InstitutionStatus
            {
                Id = 2,
                Name = "Перебуває в стані реорганізації",
                NameEn = "Undergoing reorganization",
            },
            new InstitutionStatus
            {
                Id = 3,
                Name = "Має намір на реорганізацію",
                NameEn = "Intends to reorganize",
            },
            new InstitutionStatus
            {
                Id = 4,
                Name = "Відсутній",
                NameEn = "Not available",
            });

        // default seed permissions.
        builder.Entity<PermissionsForRole>().HasData(
            new PermissionsForRole
            {
                Id = 1,
                RoleName = Role.TechAdmin.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.TechAdmin.ToString()),
                Description = "techadmin permissions",
            },
            new PermissionsForRole
            {
                Id = 2,
                RoleName = Role.Provider.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Provider.ToString()),
                Description = "provider permissions",
            },
            new PermissionsForRole
            {
                Id = 3,
                RoleName = Role.Parent.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Parent.ToString()),
                Description = "parent permissions",
            },
            new PermissionsForRole
            {
                Id = 4,
                RoleName = nameof(Role.Provider) + Constants.AdminKeyword,
                PackedPermissions = PermissionsSeeder.SeedPermissions(nameof(Role.Provider) + Constants.AdminKeyword),
                Description = "provider admin permissions",
            },
            new PermissionsForRole
            {
                Id = 5,
                RoleName = Role.MinistryAdmin.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.MinistryAdmin.ToString()),
                Description = "ministry admin permissions",
            },
            new PermissionsForRole
            {
                Id = 6,
                RoleName = Role.RegionAdmin.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.RegionAdmin.ToString()),
                Description = "region admin permissions",
            },
            new PermissionsForRole
            {
                Id = 7,
                RoleName = Role.AreaAdmin.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.AreaAdmin.ToString()),
                Description = "area admin permissions",
            }
        );

        builder.Entity<AchievementType>().HasData(
            new AchievementType
            {
                Id = 1L,
                Title = "Переможці міжнародних та всеукраїнських спортивних змагань (індивідуальних та командних)",
                TitleEn = "Winners of international and all-Ukrainian sports competitions (individual and team)",
            },
            new AchievementType
            {
                Id = 2L,
                Title = "Призери та учасники міжнародних, всеукраїнських та призери регіональних конкурсів і виставок наукових, технічних, дослідницьких, інноваційних, ІТ проектів",
                TitleEn = "Winners and participants of international, all-Ukrainian and regional contests and exhibitions of scientific, technical, research, innovation, IT projects",
            },
            new AchievementType
            {
                Id = 3L,
                Title = "Реципієнти міжнародних грантів",
                TitleEn = "Recipients of international grants",
            },
            new AchievementType
            {
                Id = 4L,
                Title = "Призери міжнародних культурних конкурсів та фестивалів",
                TitleEn = "Winners of international cultural competitions and festivals",
            },
            new AchievementType
            {
                Id = 5L,
                Title = "Соціально активні категорії учнів",
                TitleEn = "Socially active categories of students",
            },
            new AchievementType
            {
                Id = 6L,
                Title = "Цифрові інструменти Google для закладів вищої та фахової передвищої освіти",
                TitleEn = "Google digital tools for institutions of higher and professional pre-higher education",
            },
            new AchievementType
            {
                Id = 7L,
                Title = "Переможці та учасники олімпіад міжнародного та всеукраїнського рівнів",
                TitleEn = "Winners and participants of olympiads at the international and all-Ukrainian levels",
            });

        builder.Entity<ProviderType>().HasData(
            new ProviderType
            {
                Id = 1L,
                Name =
                    "Дитячо-юнацькі спортивні школи: комплексні дитячо-юнацькі спортивні школи, дитячо-юнацькі спортивні школи з видів спорту, дитячо-юнацькі спортивні школи для осіб з інвалідністю, спеціалізовані дитячо-юнацькі школи олімпійського резерву, спеціалізовані дитячо-юнацькі спортивні школи для осіб з інвалідністю паралімпійського та дефлімпійського резерву",
            },
            new ProviderType
            {
                Id = 2L,
                Name =
                    "Клуби: військово-патріотичного виховання, дитячо-юнацькі (моряків, річковиків, авіаторів, космонавтів, парашутистів, десантників, прикордонників, радистів, пожежників, автолюбителів, краєзнавців, туристів, етнографів, фольклористів, фізичної підготовки та інших напрямів)",
            },
            new ProviderType
            {
                Id = 3L,
                Name = "Мала академія мистецтв (народних ремесел)",
            },
            new ProviderType
            {
                Id = 4L,
                Name = "Мала академія наук учнівської молоді",
            },
            new ProviderType
            {
                Id = 5L,
                Name =
                    "Оздоровчі заклади для дітей та молоді: дитячо-юнацькі табори (містечка, комплекси): оздоровчі, заміські, профільні, праці та відпочинку, санаторного типу, з денним перебуванням; туристські бази",
            },
            new ProviderType
            {
                Id = 6L,
                Name = "Мистецькі школи: музична, художня, хореографічна, хорова, школа мистецтв тощо",
            },
            new ProviderType
            {
                Id = 7L,
                Name =
                    "Центр, палац, будинок, клуб художньої творчості дітей, юнацтва та молоді, художньо-естетичної творчості учнівської молоді, дитячої та юнацької творчості, естетичного виховання",
            },
            new ProviderType
            {
                Id = 8L,
                Name =
                    "Центр, будинок, клуб еколого-натуралістичної творчості учнівської молоді, станція юних натуралістів",
            },
            new ProviderType
            {
                Id = 9L,
                Name = "Центр, будинок, клуб науково-технічної творчості учнівської молоді, станція юних техніків",
            },
            new ProviderType
            {
                Id = 10L,
                Name =
                    "Центр, будинок, клуб, бюро туризму, краєзнавства, спорту та екскурсій учнівської молоді, туристсько-краєзнавчої творчості учнівської молоді, станція юних туристів",
            },
            new ProviderType
            {
                Id = 11L,
                Name = "Центри: військово-патріотичного та інших напрямів позашкільної освіти",
            },
            new ProviderType
            {
                Id = 12L,
                Name =
                    "Дитяча бібліотека, дитяча флотилія моряків і річковиків, дитячий парк, дитячий стадіон, дитячо-юнацька картинна галерея, дитячо-юнацька студія (хорова, театральна, музична, фольклорна тощо), кімната школяра, курси, студії, школи мистецтв, освітньо-культурні центри національних меншин",
            },
            new ProviderType
            {
                Id = 13L,
                Name = "Інше",
            });
    }

    /// <summary>
    /// Add configuration to reduce field sizes of Identity User and Role.
    /// </summary>
    /// <param name="builder">Model Builder.</param>
    public static void UpdateIdentityTables(this ModelBuilder builder)
    {
        builder.Entity<User>(u =>
        {
            u.Property(user => user.PhoneNumber)
                .IsUnicode(false)
                .IsFixedLength(false)
                .HasMaxLength(15);

            u.Property(user => user.PasswordHash)
                .IsUnicode(false)
                .IsFixedLength(true)
                .HasMaxLength(84);

            u.Property(user => user.SecurityStamp)
                .IsUnicode(false)
                .IsFixedLength(false)
                .HasMaxLength(36)
                .IsRequired(true);
        });
    }

    public static ModelBuilder ApplySoftDelete<T>(this ModelBuilder builder)
        where T : class, IKeyedEntity, new()
    {
        builder.Entity<T>().Property<bool>("IsDeleted").ValueGeneratedOnAdd().HasDefaultValue(false);

        builder.Entity<T>().HasIndex("IsDeleted");

        builder.Entity<T>().HasQueryFilter(m => EF.Property<bool>(m, "IsDeleted") == false);

        return builder;
    }
}