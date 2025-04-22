using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GgStatAggregator.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToTableStakeAndNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Tables_Stake_TableNumber",
                table: "Tables",
                columns: new[] { "Stake", "TableNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tables_Stake_TableNumber",
                table: "Tables");
        }
    }
}
