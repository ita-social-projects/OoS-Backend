using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class ProviderParentBlocked : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsBlocked",
            table: "ChatRoomWorkshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsBlocked",
            table: "Applications",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
                name: "BlockedProviderParents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ParentId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserIdBlock = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserIdUnblock = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateTimeFrom = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateTimeTo = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedProviderParents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedProviderParents_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlockedProviderParents_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_ParentId",
            table: "BlockedProviderParents",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_ProviderId",
            table: "BlockedProviderParents",
            column: "ProviderId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BlockedProviderParents");

        migrationBuilder.DropColumn(
            name: "IsBlocked",
            table: "ChatRoomWorkshops");

        migrationBuilder.DropColumn(
            name: "IsBlocked",
            table: "Applications");
    }
}