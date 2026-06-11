using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuildOrderUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "BuildOrders",
                newName: "BuildingType");

            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "BuildOrders",
                newName: "StartsAt");

            migrationBuilder.AddColumn<string>(
                name: "HangfireJobId",
                table: "BuildOrders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HangfireJobId",
                table: "BuildOrders");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "BuildOrders",
                newName: "StartedAt");

            migrationBuilder.RenameColumn(
                name: "BuildingType",
                table: "BuildOrders",
                newName: "Type");
        }
    }
}
