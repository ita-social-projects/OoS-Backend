using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class InstitutionHierarchyAndProviderImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
                name: "CoverImageId",
                table: "Providers",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumberOfHierarchyLevels = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ProviderImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_ProviderImages_Providers_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionFieldDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionFieldDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionFieldDescriptions_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionHierarchies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    InstitutionId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchies_InstitutionHierarchies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchies_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "DirectionInstitutionHierarchy",
                columns: table => new
                {
                    DirectionsId = table.Column<long>(type: "bigint", nullable: false),
                    InstitutionHierarchiesId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectionInstitutionHierarchy", x => new { x.DirectionsId, x.InstitutionHierarchiesId });
                    table.ForeignKey(
                        name: "FK_DirectionInstitutionHierarchy_Directions_DirectionsId",
                        column: x => x.DirectionsId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectionInstitutionHierarchy_InstitutionHierarchies_Institu~",
                        column: x => x.InstitutionHierarchiesId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_DirectionInstitutionHierarchy_InstitutionHierarchiesId",
            table: "DirectionInstitutionHierarchy",
            column: "InstitutionHierarchiesId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionFieldDescriptions_InstitutionId",
            table: "InstitutionFieldDescriptions",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_InstitutionId",
            table: "InstitutionHierarchies",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_ParentId",
            table: "InstitutionHierarchies",
            column: "ParentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DirectionInstitutionHierarchy");

        migrationBuilder.DropTable(
            name: "InstitutionFieldDescriptions");

        migrationBuilder.DropTable(
            name: "ProviderImages");

        migrationBuilder.DropTable(
            name: "InstitutionHierarchies");

        migrationBuilder.DropTable(
            name: "Institutions");

        migrationBuilder.DropColumn(
            name: "CoverImageId",
            table: "Providers");
    }
}