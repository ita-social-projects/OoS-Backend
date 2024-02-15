using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;
public partial class RemoveCity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Cities");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Cities",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                District = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Latitude = table.Column<double>(type: "double", nullable: true),
                Longitude = table.Column<double>(type: "double", nullable: true),
                Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Region = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cities", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");
    }
}
