using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class FixProviderFounderLength : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreationTime",
            table: "Ratings",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastSuccessLaunch",
            table: "QuartzJobs",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<string>(
            name: "Founder",
            table: "Providers",
            type: "varchar(60)",
            maxLength: 60,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(30)",
            oldMaxLength: 30)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "EventDateTime",
            table: "OperationsWithObjects",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ReadDateTime",
            table: "Notifications",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatedDateTime",
            table: "Notifications",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "OperationDate",
            table: "ElasticsearchSyncRecords",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ReadDateTime",
            table: "ChatMessageWorkshops",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatedDateTime",
            table: "ChatMessageWorkshops",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "DateTimeTo",
            table: "BlockedProviderParents",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "DateTimeFrom",
            table: "BlockedProviderParents",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LockoutEnd",
            table: "AspNetUsers",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastLogin",
            table: "AspNetUsers",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatingTime",
            table: "AspNetUsers",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreationTime",
            table: "Applications",
            type: "datetime(6)",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ApprovedTime",
            table: "Applications",
            type: "datetime(6)",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime",
            oldNullable: true);

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "def\n\r !()+,432578>=<?HGIFPQ[]\\rpqon|z{yx}Tg");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n2578(,PQFTn[zxy{}g");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFTxy[g");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFT[g");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreationTime",
            table: "Ratings",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastSuccessLaunch",
            table: "QuartzJobs",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<string>(
            name: "Founder",
            table: "Providers",
            type: "varchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(60)",
            oldMaxLength: 60)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "EventDateTime",
            table: "OperationsWithObjects",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ReadDateTime",
            table: "Notifications",
            type: "datetime",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatedDateTime",
            table: "Notifications",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "OperationDate",
            table: "ElasticsearchSyncRecords",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ReadDateTime",
            table: "ChatMessageWorkshops",
            type: "datetime",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatedDateTime",
            table: "ChatMessageWorkshops",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "DateTimeTo",
            table: "BlockedProviderParents",
            type: "datetime",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "DateTimeFrom",
            table: "BlockedProviderParents",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LockoutEnd",
            table: "AspNetUsers",
            type: "datetime",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "LastLogin",
            table: "AspNetUsers",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreatingTime",
            table: "AspNetUsers",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "CreationTime",
            table: "Applications",
            type: "datetime",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)");

        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "ApprovedTime",
            table: "Applications",
            type: "datetime",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "datetime(6)",
            oldNullable: true);

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "def\n\r !()+,432578>=<?HGIFPQ[]\\rpqon|z{yx}T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n2578(,PQFTn[zxy{}");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFTxy[");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFT[");
    }
}
