using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class UpdateProviderStatuses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Status",
            table: "Providers",
            type: "int",
            nullable: false,
            defaultValue: 10,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0);

        migrationBuilder.Sql(@"
            UPDATE Providers SET Status = 10 WHERE Status = 0;
            UPDATE Providers SET Status = 20 WHERE Status = 1;
            UPDATE Providers SET Status = 40 WHERE Status = 2;
            UPDATE Providers SET Status = 30 WHERE Status = 3;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Status",
            table: "Providers",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 10);

        migrationBuilder.Sql(@"
            UPDATE Providers SET Status = 0 WHERE Status = 10;
            UPDATE Providers SET Status = 1 WHERE Status = 20;
            UPDATE Providers SET Status = 2 WHERE Status = 40;
            UPDATE Providers SET Status = 3 WHERE Status = 30;");
    }
}
