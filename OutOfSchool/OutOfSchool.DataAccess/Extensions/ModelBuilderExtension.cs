using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;

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

        builder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Музичний Гурток", NameEn = "Music Workshop" },
            new Tag { Id = 2, Name = "Спортивна Секція", NameEn = "Sports Section" },
            new Tag { Id = 3, Name = "Хореографія", NameEn = "Choreography" },
            new Tag { Id = 4, Name = "Образотворче Мистецтво", NameEn = "Fine Arts" },
            new Tag { Id = 5, Name = "Театральна Студія", NameEn = "Theater Studio" },
            new Tag { Id = 6, Name = "Футбол", NameEn = "Football" },
            new Tag { Id = 7, Name = "Волейбол", NameEn = "Volleyball" },
            new Tag { Id = 8, Name = "Плавання", NameEn = "Swimming" },
            new Tag { Id = 9, Name = "Легка Атлетика", NameEn = "Track and Field" },
            new Tag { Id = 10, Name = "Баскетбол", NameEn = "Basketball" },
            new Tag { Id = 11, Name = "Гімнастика", NameEn = "Gymnastics" },
            new Tag { Id = 12, Name = "Танці", NameEn = "Dancing" },
            new Tag { Id = 13, Name = "Йога", NameEn = "Yoga" },
            new Tag { Id = 14, Name = "Карате", NameEn = "Karate" },
            new Tag { Id = 15, Name = "Айкідо", NameEn = "Aikido" },
            new Tag { Id = 16, Name = "Боротьба", NameEn = "Wrestling" },
            new Tag { Id = 17, Name = "Джудо", NameEn = "Judo" },
            new Tag { Id = 18, Name = "Кулінарія", NameEn = "Culinary Arts" },
            new Tag { Id = 19, Name = "Рукоділля", NameEn = "Handicrafts" },
            new Tag { Id = 20, Name = "Малювання", NameEn = "Drawing" },
            new Tag { Id = 21, Name = "Скульптура", NameEn = "Sculpture" },
            new Tag { Id = 22, Name = "Фотографія", NameEn = "Photography" },
            new Tag { Id = 23, Name = "Кіно Мистецтво", NameEn = "Cinema Art" },
            new Tag { Id = 24, Name = "Акторська Майстерність", NameEn = "Acting" },
            new Tag { Id = 25, Name = "Психологічні Тренінги", NameEn = "Psychological Training" },
            new Tag { Id = 26, Name = "Робототехніка", NameEn = "Robotics" },
            new Tag { Id = 27, Name = "Програмування", NameEn = "Programming" },
            new Tag { Id = 28, Name = "Інформаційні Технології", NameEn = "Information Technology" },
            new Tag { Id = 29, Name = "Шахи", NameEn = "Chess" },
            new Tag { Id = 30, Name = "Логіка", NameEn = "Logic" },
            new Tag { Id = 31, Name = "Екологія", NameEn = "Ecology" },
            new Tag { Id = 32, Name = "Наукові Дослідження", NameEn = "Scientific Research" },
            new Tag { Id = 33, Name = "Біологія", NameEn = "Biology" },
            new Tag { Id = 34, Name = "Астрономія", NameEn = "Astronomy" },
            new Tag { Id = 35, Name = "Математика", NameEn = "Mathematics" },
            new Tag { Id = 36, Name = "Фізика", NameEn = "Physics" },
            new Tag { Id = 37, Name = "Хімія", NameEn = "Chemistry" },
            new Tag { Id = 38, Name = "Іноземні Мови", NameEn = "Foreign Languages" },
            new Tag { Id = 39, Name = "Англійська Мова", NameEn = "English Language" },
            new Tag { Id = 40, Name = "Німецька Мова", NameEn = "German Language" },
            new Tag { Id = 41, Name = "Французька Мова", NameEn = "French Language" },
            new Tag { Id = 42, Name = "Іспанська Мова", NameEn = "Spanish Language" },
            new Tag { Id = 43, Name = "Журналістика", NameEn = "Journalism" },
            new Tag { Id = 44, Name = "Риторика", NameEn = "Rhetoric" },
            new Tag { Id = 45, Name = "Літературна Творчість", NameEn = "Literary Creativity" },
            new Tag { Id = 46, Name = "Історія", NameEn = "History" },
            new Tag { Id = 47, Name = "Археологія", NameEn = "Archaeology" },
            new Tag { Id = 48, Name = "Мистецтвознавство", NameEn = "Art Studies" },
            new Tag { Id = 49, Name = "Культурологія", NameEn = "Cultural Studies" },
            new Tag { Id = 50, Name = "Краєзнавство", NameEn = "Local History" },
            new Tag { Id = 51, Name = "Етнографія", NameEn = "Ethnography" },
            new Tag { Id = 52, Name = "Радіо Аматорство", NameEn = "Radio Amateur" },
            new Tag { Id = 53, Name = "Модельний Спорт", NameEn = "Model Sports" },
            new Tag { Id = 54, Name = "Авіамоделювання", NameEn = "Aeromodelling" },
            new Tag { Id = 55, Name = "Судномоделювання", NameEn = "Ship Modelling" },
            new Tag { Id = 56, Name = "Конструювання", NameEn = "Construction" },
            new Tag { Id = 57, Name = "Технічне Моделювання", NameEn = "Technical Modelling" },
            new Tag { Id = 58, Name = "Декоративно Прикладне Мистецтво", NameEn = "Decorative Arts" },
            new Tag { Id = 59, Name = "Кераміка", NameEn = "Ceramics" },
            new Tag { Id = 60, Name = "Різьба По Дереву", NameEn = "Wood Carving" },
            new Tag { Id = 61, Name = "Вишивка", NameEn = "Embroidery" },
            new Tag { Id = 62, Name = "Плетіння", NameEn = "Weaving" },
            new Tag { Id = 63, Name = "Бісероплетіння", NameEn = "Bead Weaving" },
            new Tag { Id = 64, Name = "Флористика", NameEn = "Floristry" },
            new Tag { Id = 65, Name = "Дизайн", NameEn = "Design" },
            new Tag { Id = 66, Name = "Архітектура", NameEn = "Architecture" },
            new Tag { Id = 67, Name = "Моделювання Одягу", NameEn = "Fashion Design" },
            new Tag { Id = 68, Name = "Кравецтво", NameEn = "Tailoring" },
            new Tag { Id = 69, Name = "Хенд Мейд", NameEn = "Handmade" },
            new Tag { Id = 70, Name = "Графічний Дизайн", NameEn = "Graphic Design" },
            new Tag { Id = 71, Name = "Анімація", NameEn = "Animation" },
            new Tag { Id = 72, Name = "3D Моделювання", NameEn = "3D Modelling" },
            new Tag { Id = 73, Name = "Мультиплікація", NameEn = "Cartoon Making" },
            new Tag { Id = 74, Name = "Відеомонтаж", NameEn = "Video Editing" },
            new Tag { Id = 75, Name = "Цифровий Мистецький Дизайн", NameEn = "Digital Art Design" },
            new Tag { Id = 76, Name = "Сучасне Мистецтво", NameEn = "Modern Art" },
            new Tag { Id = 77, Name = "Естрадний Спів", NameEn = "Pop Singing" },
            new Tag { Id = 78, Name = "Вокальний Ансамбль", NameEn = "Vocal Ensemble" },
            new Tag { Id = 79, Name = "Оркестр", NameEn = "Orchestra" },
            new Tag { Id = 80, Name = "Гра На Гітарі", NameEn = "Guitar Playing" },
            new Tag { Id = 81, Name = "Гра На Фортепіано", NameEn = "Piano Playing" },
            new Tag { Id = 82, Name = "Сольний Спів", NameEn = "Solo Singing" },
            new Tag { Id = 83, Name = "Хоровий Спів", NameEn = "Choral Singing" },
            new Tag { Id = 84, Name = "Фольклорний Ансамбль", NameEn = "Folklore Ensemble" },
            new Tag { Id = 85, Name = "Етнічна Музика", NameEn = "Ethnic Music" },
            new Tag { Id = 86, Name = "Духові Інструменти", NameEn = "Wind Instruments" },
            new Tag { Id = 87, Name = "Струнні Інструменти", NameEn = "String Instruments" },
            new Tag { Id = 88, Name = "Барабани", NameEn = "Drums" },
            new Tag { Id = 89, Name = "Перкусія", NameEn = "Percussion" },
            new Tag { Id = 90, Name = "Музичний Театр", NameEn = "Musical Theater" },
            new Tag { Id = 91, Name = "Сценічна Мова", NameEn = "Stage Speech" },
            new Tag { Id = 92, Name = "Імпровізація", NameEn = "Improvisation" },
            new Tag { Id = 93, Name = "Сценічний Рух", NameEn = "Stage Movement" },
            new Tag { Id = 94, Name = "Сценографія", NameEn = "Scenography" },
            new Tag { Id = 95, Name = "Художнє Читання", NameEn = "Artistic Reading" },
            new Tag { Id = 96, Name = "Модерн", NameEn = "Modern Dance" },
            new Tag { Id = 97, Name = "Балет", NameEn = "Ballet" },
            new Tag { Id = 98, Name = "Сучасні Танці", NameEn = "Modern Dances" },
            new Tag { Id = 99, Name = "Народні Танці", NameEn = "Folk Dances" },
            new Tag { Id = 100, Name = "Фітнес", NameEn = "Fitness" });

        builder.Entity<InstitutionStatus>().HasData(
            new InstitutionStatus
            {
                Id = 1,
                Name = "Працює",
                NameEn = "Active",
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
                NameEn = "Waiting for reorganization",
            },
            new InstitutionStatus
            {
                Id = 4,
                Name = "Відсутній статус",
                NameEn = "Without status",
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
                RoleName = Role.Employee.ToString(),
                PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Employee.ToString()),
                Description = "employee permissions",
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
            },
            new PermissionsForRole
            {
                 Id = 8,
                 RoleName = Role.Moderator.ToString(),
                 PackedPermissions = PermissionsSeeder.SeedPermissions(Role.Moderator.ToString()),
                 Description = "moderator permissions",
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

        builder.Entity<CompetitiveEventAccountingType>().HasData(
            new CompetitiveEventAccountingType
            {
                Id = 1,
                Title = "Освітній проєкт",
                TitleEn = "Educational project",
            },
            new CompetitiveEventAccountingType
            {
                Id = 2,
                Title = "Конкурс (не має етапів)",
                TitleEn = "Competition",
            },
            new CompetitiveEventAccountingType
            {
                Id = 3,
                Title = "Основний конкурс (має мати підпорядковані конкурси-етапи)",
                TitleEn = "Main competition",
            },
            new CompetitiveEventAccountingType
            {
                Id = 4,
                Title = "Етап конкурсу (має мати батьківський основний конкурс)",
                TitleEn = "Contest stage",
            });

        builder.Entity<CompetitiveEventCoverage>().HasData(
            new CompetitiveEventCoverage
            {
                Id = 1,
                Title = "Локальний (Шкільний)",
                TitleEn = "Local (School)",
            },
            new CompetitiveEventCoverage
            {
                Id = 2,
                Title = "Міський",
                TitleEn = "City",
            },
            new CompetitiveEventCoverage
            {
                Id = 3,
                Title = "Районний",
                TitleEn = "Raional",
            },
            new CompetitiveEventCoverage
            {
                Id = 4,
                Title = "Обласний",
                TitleEn = "Regional",
            },
            new CompetitiveEventCoverage
            {
                Id = 5,
                Title = "Всеукраїнський",
                TitleEn = "All-Ukrainian",
            },
            new CompetitiveEventCoverage
            {
                Id = 6,
                Title = "Міжнародний",
                TitleEn = "International",
            });

        builder.Entity<CompetitiveEventRegistrationDeadline>().HasData(
            new CompetitiveEventRegistrationDeadline
            {
                Id = 1,
                Title = "Постійно (протягом року)",
                TitleEn = "Constantly (during the year)",
            },
            new CompetitiveEventRegistrationDeadline
            {
                Id = 2,
                Title = "Певний місяць або місяці року",
                TitleEn = "A certain month or months of the year",
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