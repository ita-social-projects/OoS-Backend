using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class NotificationData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Notifications",
                newName: "Data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "Notifications",
                newName: "Text");
        }
    }
}
