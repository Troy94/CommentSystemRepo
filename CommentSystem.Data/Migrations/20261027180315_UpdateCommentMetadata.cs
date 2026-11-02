using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentSystem.Data.Migrations
{
    public partial class UpdateCommentMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Comments",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Comments",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Comments",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE Comments
                SET
                    UserName = CONCAT('User', Id),
                    Email = CONCAT('user', Id, '@mail.com'),
                    ParentId = NULL
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Email",
                table: "Comments",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ParentId",
                table: "Comments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserName",
                table: "Comments",
                column: "UserName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Email",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ParentId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);
        }
    }
}