using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    /// <summary>
    /// Add initial data for Social Groups.
    /// </summary>
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SocialGroups",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Діти із багатодітних сімей" },
                    { 2L, "Діти із малозабезпечених сімей" },
                    { 3L, "Діти з інвалідністю" },
                    { 4L, "Діти-сироти" },
                    { 5L, "Діти, позбавлені батьківського піклування" },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SocialGroups",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "SocialGroups",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "SocialGroups",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "SocialGroups",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "SocialGroups",
                keyColumn: "Id",
                keyValue: 5L);
        }
    }
}
