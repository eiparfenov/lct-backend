using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LctHack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ml_id",
                table: "videos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "end_time_match",
                table: "matches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "start_time_match",
                table: "matches",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ml_id",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "end_time_match",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "start_time_match",
                table: "matches");
        }
    }
}
