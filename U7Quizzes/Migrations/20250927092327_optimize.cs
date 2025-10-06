using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace U7Quizzes.Migrations
{
    /// <inheritdoc />
    public partial class optimize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response");

            migrationBuilder.DropIndex(
                name: "IX_Response_AnswerId",
                table: "Response");

            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "AnswerId",
                table: "Response");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Session");

            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                table: "Session",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AnswerId",
                table: "Response",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Response_AnswerId",
                table: "Response",
                column: "AnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response",
                column: "AnswerId",
                principalTable: "Answer",
                principalColumn: "AnswerId");
        }
    }
}
