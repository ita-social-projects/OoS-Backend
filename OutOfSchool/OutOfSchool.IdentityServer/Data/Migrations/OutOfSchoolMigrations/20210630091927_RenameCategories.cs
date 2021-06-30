using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class RenameCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Subsubcategories_SubsubcategoryId",
                table: "Workshops");

            migrationBuilder.DropTable(
                name: "Subsubcategories");

            migrationBuilder.DropTable(
                name: "Subcategories");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_SubsubcategoryId",
                table: "Workshops");

            migrationBuilder.RenameColumn(
                name: "SubsubcategoryId",
                table: "Workshops",
                newName: "ClassId");

            migrationBuilder.RenameColumn(
                name: "SubcategoryId",
                table: "Workshops",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Workshops",
                newName: "DirectionId");

            migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DirectionId = table.Column<long>(type: "bigint", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: false),
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_ClassId",
                table: "Workshops",
                column: "ClassId");

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
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Classes_ClassId",
                table: "Workshops");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Directions");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_ClassId",
                table: "Workshops");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "Workshops",
                newName: "SubsubcategoryId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Workshops",
                newName: "SubcategoryId");

            migrationBuilder.RenameColumn(
                name: "DirectionId",
                table: "Workshops",
                newName: "CategoryId");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subcategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subcategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subsubcategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubcategoryId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subsubcategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subsubcategories_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_SubsubcategoryId",
                table: "Workshops",
                column: "SubsubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subsubcategories_SubcategoryId",
                table: "Subsubcategories",
                column: "SubcategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Subsubcategories_SubsubcategoryId",
                table: "Workshops",
                column: "SubsubcategoryId",
                principalTable: "Subsubcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }
    }
}
