using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class TagSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 7L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 9L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 10L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 11L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 12L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 13L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 14L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 15L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 16L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 17L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 18L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 19L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 20L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 21L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 22L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 23L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 24L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 25L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 26L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 27L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 28L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 29L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 30L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 31L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 32L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 33L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 34L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 35L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 36L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 37L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 38L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 39L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 40L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 41L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 42L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 43L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 44L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 45L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 46L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 47L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 48L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 49L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 50L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 51L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 52L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 53L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 54L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 55L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 56L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 57L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 58L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 59L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 60L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 61L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 62L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 63L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 64L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 65L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 66L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 67L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 68L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 69L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 70L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 71L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 72L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 73L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 74L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 75L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 76L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 77L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 78L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 79L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 80L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 81L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 82L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 83L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 84L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 85L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 86L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 87L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 88L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 89L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 90L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 91L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 92L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 93L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 94L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 95L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 96L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 97L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 98L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 99L);

            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: 100L);
        }
    }
}
