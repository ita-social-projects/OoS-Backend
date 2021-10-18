using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class InstitutionStatusAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "InstitutionStatusId",
                table: "Providers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstitutionStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionStatuses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InstitutionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1L, "Працює" });

            migrationBuilder.InsertData(
                table: "InstitutionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2L, "Перебуває в стані реорганізації" });

            migrationBuilder.InsertData(
                table: "InstitutionStatuses",
                columns: new[] { "Id", "Name" },
                values: new object[] { 3L, "Має намір на реорганізацію" });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_InstitutionStatusId",
                table: "Providers",
                column: "InstitutionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_InstitutionStatuses_InstitutionStatusId",
                table: "Providers",
                column: "InstitutionStatusId",
                principalTable: "InstitutionStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_InstitutionStatuses_InstitutionStatusId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "InstitutionStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Providers_InstitutionStatusId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "InstitutionStatusId",
                table: "Providers");
        }
    }
}
