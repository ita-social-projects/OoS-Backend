using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddAverageRatingsTableRemoveTypeColumnFromRatingsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Type",
            table: "Ratings");

        migrationBuilder.CreateTable(
            name: "AverageRatings",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Rate = table.Column<float>(type: "float", nullable: false),
                RateQuantity = table.Column<int>(type: "int", nullable: false),
                EntityId = table.Column<Guid>(type: "binary(16)", nullable: false),
                CreationTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AverageRatings", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_AverageRatings_EntityId",
            table: "AverageRatings",
            column: "EntityId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AverageRatings");

        migrationBuilder.AddColumn<int>(
            name: "Type",
            table: "Ratings",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }
}
