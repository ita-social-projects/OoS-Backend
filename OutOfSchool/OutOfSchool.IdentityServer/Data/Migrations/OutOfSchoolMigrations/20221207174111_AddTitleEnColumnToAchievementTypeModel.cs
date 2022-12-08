using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddTitleEnColumnToAchievementTypeModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TitleEn",
            table: "AchievementTypes",
            type: "varchar(200)",
            maxLength: 200,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TitleEn",
            table: "AchievementTypes");
    }
}
