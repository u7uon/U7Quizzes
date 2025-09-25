using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace U7Quizzes.Migrations
{
    /// <inheritdoc />
    public partial class addCL_connectionID_Participant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Participant",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Participant");
        }
    }
}
