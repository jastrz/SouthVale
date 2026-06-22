using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobIdToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "TrainOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobId",
                table: "BuildOrders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobId",
                table: "TrainOrders");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "BuildOrders");
        }
    }
}
