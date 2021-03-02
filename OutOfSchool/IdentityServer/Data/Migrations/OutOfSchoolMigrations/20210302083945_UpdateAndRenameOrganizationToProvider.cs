using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class UpdateAndRenameOrganizationToProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Organizations_OrganizationId",
                table: "Workshops");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "Workshops",
                newName: "ProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_Workshops_OrganizationId",
                table: "Workshops",
                newName: "IX_Workshops_ProviderId");

            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortTitle = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    MFO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EDRPOU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KOATUU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    INPP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Director = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DirectorPosition = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AuthorityHolder = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DirectorPhonenumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ManagerialBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ownership = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profile = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    isSubmitPZ_1 = table.Column<bool>(type: "bit", nullable: false),
                    AttachedDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Providers_UserId",
                table: "Providers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops");

            migrationBuilder.DropTable(
                name: "Providers");

            migrationBuilder.RenameColumn(
                name: "ProviderId",
                table: "Workshops",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_Workshops_ProviderId",
                table: "Workshops",
                newName: "IX_Workshops_OrganizationId");

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: false),
                    EDRPOU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    INPP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MFO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_UserId",
                table: "Organizations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Organizations_OrganizationId",
                table: "Workshops",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
