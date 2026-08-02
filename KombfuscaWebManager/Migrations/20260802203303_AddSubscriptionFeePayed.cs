using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KombfuscaWebManager.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionFeePayed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SubscriptionFeePayed",
                table: "Participations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionFeePayed",
                table: "Participations");
        }
    }
}
