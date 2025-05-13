#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedAttributesCompEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingHoldingId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "ChildParticipantId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "NumberOfOccupiedSeats",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "CompetitiveEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BuildingHoldingId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChildParticipantId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "NumberOfOccupiedSeats",
                table: "CompetitiveEvents",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true);
        }
    }
}
