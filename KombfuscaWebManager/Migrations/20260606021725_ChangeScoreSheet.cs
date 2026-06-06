using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KombfuscaWebManager.Migrations
{
    /// <inheritdoc />
    public partial class ChangeScoreSheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_UserId",
                table: "ScoreSheets");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ScoreSheets",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "ScoreSheets",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreSheets_ApplicationUserId",
                table: "ScoreSheets",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreSheets_CreatedByUserId",
                table: "ScoreSheets",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_ApplicationUserId",
                table: "ScoreSheets",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_CreatedByUserId",
                table: "ScoreSheets",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_UserId",
                table: "ScoreSheets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_ApplicationUserId",
                table: "ScoreSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_CreatedByUserId",
                table: "ScoreSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_UserId",
                table: "ScoreSheets");

            migrationBuilder.DropIndex(
                name: "IX_ScoreSheets_ApplicationUserId",
                table: "ScoreSheets");

            migrationBuilder.DropIndex(
                name: "IX_ScoreSheets_CreatedByUserId",
                table: "ScoreSheets");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ScoreSheets");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScoreSheets");

            migrationBuilder.AddForeignKey(
                name: "FK_ScoreSheets_AspNetUsers_UserId",
                table: "ScoreSheets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
