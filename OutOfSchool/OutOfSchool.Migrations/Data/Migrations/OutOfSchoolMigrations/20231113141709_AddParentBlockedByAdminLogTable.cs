using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddParentBlockedByAdminLogTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ParentBlockedByAdminLog",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                ParentId = table.Column<Guid>(type: "binary(16)", nullable: false),
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ParentBlockedByAdminLog", x => x.Id);
                table.ForeignKey(
                    name: "FK_ParentBlockedByAdminLog_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ParentBlockedByAdminLog_Parents_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Parents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_ParentBlockedByAdminLog_ParentId",
            table: "ParentBlockedByAdminLog",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ParentBlockedByAdminLog_UserId",
            table: "ParentBlockedByAdminLog",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ParentBlockedByAdminLog");
    }
}
