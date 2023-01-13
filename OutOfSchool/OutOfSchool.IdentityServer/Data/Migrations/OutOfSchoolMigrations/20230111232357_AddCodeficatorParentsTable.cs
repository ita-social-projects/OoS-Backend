using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddCodeficatorParentsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CodeficatorParents",
            columns: table => new
            {
                CatottgsId = table.Column<long>(type: "bigint", nullable: false),
                ParentId = table.Column<long>(type: "bigint", nullable: false),
                Level = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.ForeignKey(
                    name: "FK_CodeficatorParents_CATOTTGs_CatottgsId",
                    column: x => x.CatottgsId,
                    principalTable: "CATOTTGs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CodeficatorParents_CATOTTGs_ParentId",
                    column: x => x.ParentId,
                    principalTable: "CATOTTGs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_CodeficatorParents_CatottgsId",
            table: "CodeficatorParents",
            column: "CatottgsId");

        migrationBuilder.CreateIndex(
            name: "IX_CodeficatorParents_ParentId",
            table: "CodeficatorParents",
            column: "ParentId");

        migrationBuilder.Sql(@"
        DROP TRIGGER IF EXISTS catottgs_AFTER_DELETE;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_DELETE AFTER DELETE ON catottgs FOR EACH ROW
        BEGIN
	        DELETE FROM CodeficatorParents WHERE CatottgsId = old.Id;	
	        DELETE FROM CodeficatorParents WHERE ParentId = old.Id;
        END;");

        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_INSERT;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_INSERT AFTER INSERT ON catottgs FOR EACH ROW
        BEGIN
	        INSERT INTO CodeficatorParents (CatottgsId, ParentId, Level)(
	        WITH recursive parent_users (Id, Parentid, Level) AS (
	          SELECT new.Id, new.ParentId, 1 Level
	          UNION ALL
	          SELECT t.Id, t.ParentId, Level + 1
	          FROM CATOTTGs t INNER JOIN parent_users pu
	          ON t.Id = pu.ParentId
	        )
	        SELECT new.Id, ParentId, Level FROM parent_users WHERE ParentId IS NOT NULL);
        END;");

        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_UPDATE;

        CREATE DEFINER = CURRENT_USER TRIGGER catottgs_AFTER_UPDATE AFTER UPDATE ON catottgs FOR EACH ROW
        BEGIN
	        IF (new.ParentId <> old.ParentId OR new.Id <> old.Id) THEN
		        DELETE 
	                c1 
                FROM 
	                CodeficatorParents c1 
                    INNER JOIN  (SELECT CatottgsId FROM CodeficatorParents WHERE ParentId = old.Id
			
			                UNION
			
			                SELECT new.Id
            
			                UNION
            
			                SELECT old.Id) c2 
		                ON c2.CatottgsId = c1.CatottgsId;
        
                INSERT INTO CodeficatorParents (CatottgsId, ParentId, Level)(
		        WITH recursive parent_users (Id, ParentId, Level) AS (
		          SELECT Id, ParentId, 1 Level
                  FROM CATOTTGs t
                  WHERE Id IN (SELECT CatottgsId From CodeficatorParents WHERE ParentId = old.Id
			
			            UNION
			
			            SELECT new.Id
            
			            UNION
            
			            SELECT old.Id)
		          UNION ALL
		          SELECT pu.Id, t.ParentId, Level + 1
		          FROM CATOTTGs t INNER JOIN parent_users pu
		          ON t.Id = pu.ParentId
		        )
		        SELECT Id, ParentId, Level FROM parent_users WHERE ParentId IS NOT NULL);        
	        END IF;
        END;");

        migrationBuilder.Sql(@"
	    DELETE FROM CodeficatorParents;

        INSERT INTO CodeficatorParents (CatottgsId, ParentId, Level)(
        WITH RECURSIVE parent_users (Id, ParentId, Level) AS (
          SELECT Id, ParentId, 1 Level
          FROM CATOTTGs t
          UNION ALL
          SELECT pu.id, t.parentid, level + 1
          FROM CATOTTGs t INNER JOIN parent_users pu
          ON t.Id = pu.ParentId
        )
        SELECT Id, ParentId, Level FROM parent_users WHERE ParentId IS NOT NULL);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
	    DROP TRIGGER IF EXISTS catottgs_AFTER_UPDATE;

        DROP TRIGGER IF EXISTS catottgs_AFTER_INSERT;

        DROP TRIGGER IF EXISTS catottgs_AFTER_DELETE;");

        migrationBuilder.DropTable(
            name: "CodeficatorParents");
    }
}
