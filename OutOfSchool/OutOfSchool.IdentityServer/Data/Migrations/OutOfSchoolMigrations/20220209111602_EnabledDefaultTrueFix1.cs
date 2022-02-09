using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class EnabledDefaultTrueFix1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
