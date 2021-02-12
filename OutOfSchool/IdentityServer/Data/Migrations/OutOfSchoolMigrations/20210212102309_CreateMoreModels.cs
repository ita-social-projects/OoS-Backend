using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class CreateMoreModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Parents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Image",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Organizations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingNumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId);
                });

            migrationBuilder.CreateTable(
                name: "DirectionsOfEducation",
                columns: table => new
                {
                    DirectionOfEducationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectionsOfEducation", x => x.DirectionOfEducationId);
                });

            migrationBuilder.CreateTable(
                name: "SocialGroup",
                columns: table => new
                {
                    SocialGroupId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialGroup", x => x.SocialGroupId);
                });

            migrationBuilder.CreateTable(
                name: "ProfilesOfEducation",
                columns: table => new
                {
                    ProfileOfEducationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectionOfEducationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfilesOfEducation", x => x.ProfileOfEducationId);
                    table.ForeignKey(
                        name: "FK_ProfilesOfEducation_DirectionsOfEducation_DirectionOfEducationId",
                        column: x => x.DirectionOfEducationId,
                        principalTable: "DirectionsOfEducation",
                        principalColumn: "DirectionOfEducationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    SectionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Istagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    MaxAge = table.Column<int>(type: "int", nullable: false),
                    DaysPerWeek = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WithDisabilityOptions = table.Column<bool>(type: "bit", nullable: false),
                    DisabilityOptionsDesc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    DirectionOfEducationId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.SectionId);
                    table.ForeignKey(
                        name: "FK_Sections_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sections_DirectionsOfEducation_DirectionOfEducationId",
                        column: x => x.DirectionOfEducationId,
                        principalTable: "DirectionsOfEducation",
                        principalColumn: "DirectionOfEducationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    ChildId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    SocialGroupId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.ChildId);
                    table.ForeignKey(
                        name: "FK_Children_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "ParentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Children_SocialGroup_SocialGroupId",
                        column: x => x.SocialGroupId,
                        principalTable: "SocialGroup",
                        principalColumn: "SocialGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    TeacherId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.TeacherId);
                    table.ForeignKey(
                        name: "FK_Teachers_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "SectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parents_UserId",
                table: "Parents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_UserId",
                table: "Organizations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Children_ParentId",
                table: "Children",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Children_SocialGroupId",
                table: "Children",
                column: "SocialGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfilesOfEducation_DirectionOfEducationId",
                table: "ProfilesOfEducation",
                column: "DirectionOfEducationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_AddressId",
                table: "Sections",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_DirectionOfEducationId",
                table: "Sections",
                column: "DirectionOfEducationId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_SectionId",
                table: "Teachers",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_AspNetUsers_UserId",
                table: "Organizations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_AspNetUsers_UserId",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Parents_AspNetUsers_UserId",
                table: "Parents");

            migrationBuilder.DropTable(
                name: "Children");

            migrationBuilder.DropTable(
                name: "ProfilesOfEducation");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "SocialGroup");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "DirectionsOfEducation");

            migrationBuilder.DropIndex(
                name: "IX_Parents_UserId",
                table: "Parents");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_UserId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Parents");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Organizations");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Image",
                table: "Organizations",
                type: "varbinary(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
