using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class Provider_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Providers");

            migrationBuilder.AddColumn<long>(
                name: "TypeId",
                table: "Providers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ProviderTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ProviderTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1L, "Дитячо-юнацькі спортивні школи: комплексні дитячо-юнацькі спортивні школи, дитячо-юнацькі спортивні школи з видів спорту, дитячо-юнацькі спортивні школи для осіб з інвалідністю, спеціалізовані дитячо-юнацькі школи олімпійського резерву, спеціалізовані дитячо-юнацькі спортивні школи для осіб з інвалідністю паралімпійського та дефлімпійського резерву" },
                    { 2L, "Клуби: військово-патріотичного виховання, дитячо-юнацькі (моряків, річковиків, авіаторів, космонавтів, парашутистів, десантників, прикордонників, радистів, пожежників, автолюбителів, краєзнавців, туристів, етнографів, фольклористів, фізичної підготовки та інших напрямів)" },
                    { 3L, "Мала академія мистецтв (народних ремесел)" },
                    { 4L, "Мала академія наук учнівської молоді" },
                    { 5L, "Оздоровчі заклади для дітей та молоді: дитячо-юнацькі табори (містечка, комплекси): оздоровчі, заміські, профільні, праці та відпочинку, санаторного типу, з денним перебуванням; туристські бази" },
                    { 6L, "Мистецькі школи: музична, художня, хореографічна, хорова, школа мистецтв тощо" },
                    { 7L, "Центр, палац, будинок, клуб художньої творчості дітей, юнацтва та молоді, художньо-естетичної творчості учнівської молоді, дитячої та юнацької творчості, естетичного виховання" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_TypeId",
                table: "Providers",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_ProviderTypes_TypeId",
                table: "Providers",
                column: "TypeId",
                principalTable: "ProviderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_ProviderTypes_TypeId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "ProviderTypes");

            migrationBuilder.DropIndex(
                name: "IX_Providers_TypeId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Providers");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
