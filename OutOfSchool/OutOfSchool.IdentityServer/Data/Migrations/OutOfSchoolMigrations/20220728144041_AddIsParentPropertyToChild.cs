using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddIsParentPropertyToChild : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsParent",
            table: "Children",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(@"
            INSERT INTO Children (Id, FirstName, LastName, MiddleName, Gender, ParentId, IsParent, DateOfBirth)
            SELECT UUID_TO_BIN(UUID()), u.FirstName, u.LastName, IFNULL(u.MiddleName, ''), u.Gender, p.Id, 1, '2001-01-01'
            FROM AspNetUsers u
	            INNER JOIN Parents p ON p.UserId = u.Id
                LEFT JOIN Children c ON c.ParentId = p.Id AND c.IsParent = 1
            WHERE u.Role = 'parent' AND c.Id IS NULL;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM children
            WHERE IsParent = 1;");

        migrationBuilder.DropColumn(
            name: "IsParent",
            table: "Children");
    }
}