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

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subsubcategories",
                table: "Subsubcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Subsubcategories_Subcategories_SubcategoryId",
                table: "Subsubcategories");

            migrationBuilder.DropIndex(
                name: "IX_Subsubcategories_SubcategoryId",
                table: "Subsubcategories");

            migrationBuilder.RenameTable(
                name: "Subsubcategories",
                newName: "Classes");

            migrationBuilder.RenameColumn(
                name: "SubcategoryId",
                table: "Classes",
                newName: "DepartmentId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subcategories",
                table: "Subcategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Subcategories_Categories_CategoryId",
                table: "Subcategories");

            migrationBuilder.DropIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories");

            migrationBuilder.RenameTable(
                name: "Subcategories",
                newName: "Departments");

            migrationBuilder.RenameColumn(
               name: "CategoryId",
               table: "Departments",
               newName: "DirectionId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Directions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Directions",
                table: "Directions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departments",
                table: "Departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Classes",
                table: "Classes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Directions_DirectionId",
                table: "Departments",
                column: "DirectionId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DirectionId",
                table: "Departments",
                column: "DirectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Departments_DepartmentId",
                table: "Classes",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_DepartmentId",
                table: "Classes",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Classes_ClassId",
                table: "Workshops",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_ClassId",
                table: "Workshops",
                column: "ClassId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Classes_ClassId",
                table: "Workshops");

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

            migrationBuilder.DropPrimaryKey(
                name: "PK_Classes",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Departments_DepartmentId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_DepartmentId",
                table: "Classes");

            migrationBuilder.RenameTable(
                name: "Classes",
                newName: "Subsubcategories");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "Subsubcategories",
                newName: "SubcategoryId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departments",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Directions_DirectionId",
                table: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DirectionId",
                table: "Departments");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "Subcategories");

            migrationBuilder.RenameColumn(
               name: "DirectionId",
               table: "Subcategories",
               newName: "CategoryId");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Directions",
                table: "Directions");

            migrationBuilder.RenameTable(
                name: "Directions",
                newName: "Categories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subcategories",
                table: "Subcategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subsubcategories",
                table: "Subsubcategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subcategories_Categories_CategoryId",
                table: "Subcategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId",
                table: "Subcategories",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subsubcategories_Subcategories_SubcategoryId",
                table: "Subsubcategories",
                column: "SubcategoryId",
                principalTable: "Subcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_SubsubcategoryId",
                table: "Workshops",
                column: "SubsubcategoryId");
        }
    }
}
