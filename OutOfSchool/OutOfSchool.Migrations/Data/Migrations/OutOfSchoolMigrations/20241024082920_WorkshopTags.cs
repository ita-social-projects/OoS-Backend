using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class WorkshopTags : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameEn = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "TagWorkshop",
                columns: table => new
                {
                    TagsId = table.Column<long>(type: "bigint", nullable: false),
                    WorkshopsId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagWorkshop", x => new { x.TagsId, x.WorkshopsId });
                    table.ForeignKey(
                        name: "FK_TagWorkshop_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagWorkshop_Workshops_WorkshopsId",
                        column: x => x.WorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.InsertData(
            table: "Tags",
            columns: new[] { "Id", "Name", "NameEn" },
            values: new object[,]
            {
                { 1L, "Музичний Гурток", "Music Workshop" },
                { 2L, "Спортивна Секція", "Sports Section" },
                { 3L, "Хореографія", "Choreography" },
                { 4L, "Образотворче Мистецтво", "Fine Arts" },
                { 5L, "Театральна Студія", "Theater Studio" },
                { 6L, "Футбол", "Football" },
                { 7L, "Волейбол", "Volleyball" },
                { 8L, "Плавання", "Swimming" },
                { 9L, "Легка Атлетика", "Track and Field" },
                { 10L, "Баскетбол", "Basketball" },
                { 11L, "Гімнастика", "Gymnastics" },
                { 12L, "Танці", "Dancing" },
                { 13L, "Йога", "Yoga" },
                { 14L, "Карате", "Karate" },
                { 15L, "Айкідо", "Aikido" },
                { 16L, "Боротьба", "Wrestling" },
                { 17L, "Джудо", "Judo" },
                { 18L, "Кулінарія", "Culinary Arts" },
                { 19L, "Рукоділля", "Handicrafts" },
                { 20L, "Малювання", "Drawing" },
                { 21L, "Скульптура", "Sculpture" },
                { 22L, "Фотографія", "Photography" },
                { 23L, "Кіно Мистецтво", "Cinema Art" },
                { 24L, "Акторська Майстерність", "Acting" },
                { 25L, "Психологічні Тренінги", "Psychological Training" },
                { 26L, "Робототехніка", "Robotics" },
                { 27L, "Програмування", "Programming" },
                { 28L, "Інформаційні Технології", "Information Technology" },
                { 29L, "Шахи", "Chess" },
                { 30L, "Логіка", "Logic" },
                { 31L, "Екологія", "Ecology" },
                { 32L, "Наукові Дослідження", "Scientific Research" },
                { 33L, "Біологія", "Biology" },
                { 34L, "Астрономія", "Astronomy" },
                { 35L, "Математика", "Mathematics" },
                { 36L, "Фізика", "Physics" },
                { 37L, "Хімія", "Chemistry" },
                { 38L, "Іноземні Мови", "Foreign Languages" },
                { 39L, "Англійська Мова", "English Language" },
                { 40L, "Німецька Мова", "German Language" },
                { 41L, "Французька Мова", "French Language" },
                { 42L, "Іспанська Мова", "Spanish Language" },
                { 43L, "Журналістика", "Journalism" },
                { 44L, "Риторика", "Rhetoric" },
                { 45L, "Літературна Творчість", "Literary Creativity" },
                { 46L, "Історія", "History" },
                { 47L, "Археологія", "Archaeology" },
                { 48L, "Мистецтвознавство", "Art Studies" },
                { 49L, "Культурологія", "Cultural Studies" },
                { 50L, "Краєзнавство", "Local History" },
                { 51L, "Етнографія", "Ethnography" },
                { 52L, "Радіо Аматорство", "Radio Amateur" },
                { 53L, "Модельний Спорт", "Model Sports" },
                { 54L, "Авіамоделювання", "Aeromodelling" },
                { 55L, "Судномоделювання", "Ship Modelling" },
                { 56L, "Конструювання", "Construction" },
                { 57L, "Технічне Моделювання", "Technical Modelling" },
                { 58L, "Декоративно Прикладне Мистецтво", "Decorative Arts" },
                { 59L, "Кераміка", "Ceramics" },
                { 60L, "Різьба По Дереву", "Wood Carving" },
                { 61L, "Вишивка", "Embroidery" },
                { 62L, "Плетіння", "Weaving" },
                { 63L, "Бісероплетіння", "Bead Weaving" },
                { 64L, "Флористика", "Floristry" },
                { 65L, "Дизайн", "Design" },
                { 66L, "Архітектура", "Architecture" },
                { 67L, "Моделювання Одягу", "Fashion Design" },
                { 68L, "Кравецтво", "Tailoring" },
                { 69L, "Хенд Мейд", "Handmade" },
                { 70L, "Графічний Дизайн", "Graphic Design" },
                { 71L, "Анімація", "Animation" },
                { 72L, "3D Моделювання", "3D Modelling" },
                { 73L, "Мультиплікація", "Cartoon Making" },
                { 74L, "Відеомонтаж", "Video Editing" },
                { 75L, "Цифровий Мистецький Дизайн", "Digital Art Design" },
                { 76L, "Сучасне Мистецтво", "Modern Art" },
                { 77L, "Естрадний Спів", "Pop Singing" },
                { 78L, "Вокальний Ансамбль", "Vocal Ensemble" },
                { 79L, "Оркестр", "Orchestra" },
                { 80L, "Гра На Гітарі", "Guitar Playing" },
                { 81L, "Гра На Фортепіано", "Piano Playing" },
                { 82L, "Сольний Спів", "Solo Singing" },
                { 83L, "Хоровий Спів", "Choral Singing" },
                { 84L, "Фольклорний Ансамбль", "Folklore Ensemble" },
                { 85L, "Етнічна Музика", "Ethnic Music" },
                { 86L, "Духові Інструменти", "Wind Instruments" },
                { 87L, "Струнні Інструменти", "String Instruments" },
                { 88L, "Барабани", "Drums" },
                { 89L, "Перкусія", "Percussion" },
                { 90L, "Музичний Театр", "Musical Theater" },
                { 91L, "Сценічна Мова", "Stage Speech" },
                { 92L, "Імпровізація", "Improvisation" },
                { 93L, "Сценічний Рух", "Stage Movement" },
                { 94L, "Сценографія", "Scenography" },
                { 95L, "Художнє Читання", "Artistic Reading" },
                { 96L, "Модерн", "Modern Dance" },
                { 97L, "Балет", "Ballet" },
                { 98L, "Сучасні Танці", "Modern Dances" },
                { 99L, "Народні Танці", "Folk Dances" },
                { 100L, "Фітнес", "Fitness" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_TagWorkshop_WorkshopsId",
            table: "TagWorkshop",
            column: "WorkshopsId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TagWorkshop");

        migrationBuilder.DropTable(
            name: "Tags");
    }
}