using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KombfuscaWebManager.Migrations
{
    /// <inheritdoc />
    public partial class FixAdModelsProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdSubscriptionPeriodId",
                table: "AdCategories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdCategories_AdSubscriptionPeriodId",
                table: "AdCategories",
                column: "AdSubscriptionPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdCategories_AdSubscriptionPeriods_AdSubscriptionPeriodId",
                table: "AdCategories",
                column: "AdSubscriptionPeriodId",
                principalTable: "AdSubscriptionPeriods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdCategories_AdSubscriptionPeriods_AdSubscriptionPeriodId",
                table: "AdCategories");

            migrationBuilder.DropIndex(
                name: "IX_AdCategories_AdSubscriptionPeriodId",
                table: "AdCategories");

            migrationBuilder.DropColumn(
                name: "AdSubscriptionPeriodId",
                table: "AdCategories");
        }
    }
}
