using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationPriceAndPeriodicity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Tasks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Tasks",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurringDayOfWeek",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurringRepetitions",
                table: "Tasks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RecurringDayOfWeek",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RecurringRepetitions",
                table: "Tasks");
        }
    }
}
