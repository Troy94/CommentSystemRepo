using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentSystem.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Text = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    CreatedAt = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                });

            migrationBuilder.Sql(@"
                DECLARE @i INT = 1;

                WHILE @i <= 50
                BEGIN
                    INSERT INTO Comments
                    (
                        Text,
                        CreatedAt
                    )
                    VALUES
                    (
                        CONCAT('Seed comment #', @i),
                        DATEADD(MINUTE, @i, '2026-10-01T09:00:00')
                    );

                    SET @i = @i + 1;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");
        }
    }
}