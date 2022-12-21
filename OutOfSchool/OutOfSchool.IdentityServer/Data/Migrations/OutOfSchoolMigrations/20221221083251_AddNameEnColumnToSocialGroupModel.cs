using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddNameEnColumnToSocialGroupModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Name",
            keyValue: null,
            column: "Name",
            value: "");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(100)",
            oldMaxLength: 100,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "NameEn",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 1L,
            column: "TitleEn",
            value: "Winners of international and all-Ukrainian sports competitions (individual and team)");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 2L,
            column: "TitleEn",
            value: "Winners and participants of international, all-Ukrainian and regional contests and exhibitions of scientific, technical, research, innovation, IT projects");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 3L,
            column: "TitleEn",
            value: "Recipients of international grants");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 4L,
            column: "TitleEn",
            value: "Winners of international cultural competitions and festivals");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 5L,
            column: "TitleEn",
            value: "Socially active categories of students");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 6L,
            column: "TitleEn",
            value: "Google digital tools for institutions of higher and professional pre-higher education");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 7L,
            column: "TitleEn",
            value: "Winners and participants of olympiads at the international and all-Ukrainian levels");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 1L,
            column: "NameEn",
            value: "Children from large families");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 2L,
            column: "NameEn",
            value: "Children from low-income families");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 3L,
            column: "NameEn",
            value: "Children with disabilities");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 4L,
            column: "NameEn",
            value: "Orphans");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 5L,
            column: "NameEn",
            value: "Children deprived of parental care");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NameEn",
            table: "SocialGroups");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(100)",
            oldMaxLength: 100)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 1L,
            column: "TitleEn",
            value: "Peremozhtsi mizhnarodnykh ta vseukrainskykh sportyvnykh zmahan (indyvidualnykh ta komandnykh)");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 2L,
            column: "TitleEn",
            value: "Pryzery ta uchasnyky mizhnarodnykh, vseukrainskykh ta pryzery rehionalnykh konkursiv i vystavok naukovykh, tekhnichnykh, doslidnytskykh, innovatsiinykh, IT proektiv");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 3L,
            column: "TitleEn",
            value: "Retsypiienty mizhnarodnykh hrantiv");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 4L,
            column: "TitleEn",
            value: "Pryzery mizhnarodnykh kulturnykh konkursiv ta festyvaliv");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 5L,
            column: "TitleEn",
            value: "Sotsialno aktyvni katehorii uchniv");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 6L,
            column: "TitleEn",
            value: "Tsyfrovi instrumenty Google dlia zakladiv vyshchoi ta fakhovoi peredvyshchoi osvity");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 7L,
            column: "TitleEn",
            value: "Peremozhtsi ta uchasnyky olimpiad mizhnarodnoho ta vseukrainskoho rivniv");
    }
}
