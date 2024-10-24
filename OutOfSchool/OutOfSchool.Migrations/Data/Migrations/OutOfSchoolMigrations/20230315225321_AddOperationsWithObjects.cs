using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddOperationsWithObjects : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "OperationsWithObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    EntityId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    RowSeparator = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsWithObjects", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_EntityId",
            table: "OperationsWithObjects",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_EntityType",
            table: "OperationsWithObjects",
            column: "EntityType");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_OperationType",
            table: "OperationsWithObjects",
            column: "OperationType");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_RowSeparator",
            table: "OperationsWithObjects",
            column: "RowSeparator");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OperationsWithObjects");
    }
}