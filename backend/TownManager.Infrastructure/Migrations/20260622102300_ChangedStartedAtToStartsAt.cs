using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedStartedAtToStartsAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartedAt",
                table: "TrainOrders",
                newName: "StartsAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "TrainOrders",
                newName: "StartedAt");
        }
    }
}
