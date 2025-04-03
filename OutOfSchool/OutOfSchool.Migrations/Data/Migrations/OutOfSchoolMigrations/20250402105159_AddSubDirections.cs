#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddSubDirections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DirectionInstitutionHierarchy");

            migrationBuilder.CreateTable(
                name: "SubDirections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DirectionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDirections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubDirections_Directions_DirectionId",
                        column: x => x.DirectionId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InstitutionHierarchySubDirection",
                columns: table => new
                {
                    InstitutionHierarchiesId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    SubDirectionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionHierarchySubDirection", x => new { x.InstitutionHierarchiesId, x.SubDirectionsId });
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchySubDirection_InstitutionHierarchies_Inst~",
                        column: x => x.InstitutionHierarchiesId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchySubDirection_SubDirections_SubDirections~",
                        column: x => x.SubDirectionsId,
                        principalTable: "SubDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionHierarchySubDirection_SubDirectionsId",
                table: "InstitutionHierarchySubDirection",
                column: "SubDirectionsId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDirections_DirectionId",
                table: "SubDirections",
                column: "DirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDirections_IsDeleted",
                table: "SubDirections",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstitutionHierarchySubDirection");

            migrationBuilder.DropTable(
                name: "SubDirections");

            migrationBuilder.CreateTable(
                name: "DirectionInstitutionHierarchy",
                columns: table => new
                {
                    DirectionsId = table.Column<long>(type: "bigint", nullable: false),
                    InstitutionHierarchiesId = table.Column<Guid>(type: "UUID(16)", nullable: false)
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
        }
    }
}
