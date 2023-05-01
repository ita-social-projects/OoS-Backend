using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class RemoveClassDepartment : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Classes_ClassId",
            table: "Workshops");

        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops");

        migrationBuilder.DropTable(
            name: "Classes");

        migrationBuilder.DropTable(
            name: "Departments");

        migrationBuilder.DropIndex(
            name: "IX_Workshops_ClassId",
            table: "Workshops");

        migrationBuilder.DropIndex(
            name: "IX_Workshops_DirectionId",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "ClassId",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "DepartmentId",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "DirectionId",
            table: "Workshops");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "ClassId",
            table: "Workshops",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "DepartmentId",
            table: "Workshops",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<long>(
            name: "DirectionId",
            table: "Workshops",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DirectionId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Directions_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_ClassId",
            table: "Workshops",
            column: "ClassId");

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_DirectionId",
            table: "Workshops",
            column: "DirectionId");

        migrationBuilder.CreateIndex(
            name: "IX_Classes_DepartmentId",
            table: "Classes",
            column: "DepartmentId");

        migrationBuilder.CreateIndex(
            name: "IX_Departments_DirectionId",
            table: "Departments",
            column: "DirectionId");

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Classes_ClassId",
            table: "Workshops",
            column: "ClassId",
            principalTable: "Classes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops",
            column: "DirectionId",
            principalTable: "Directions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}