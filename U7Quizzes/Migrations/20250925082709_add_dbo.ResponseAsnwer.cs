using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace U7Quizzes.Migrations
{
    /// <inheritdoc />
    public partial class add_dboResponseAsnwer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response");

            migrationBuilder.AlterColumn<string>(
                name: "TextResponse",
                table: "Response",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                table: "Participant",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ResponseAnswer",
                columns: table => new
                {
                    ResponseId = table.Column<int>(type: "int", nullable: false),
                    AnswerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseAnswer", x => new { x.ResponseId, x.AnswerId });
                    table.ForeignKey(
                        name: "FK_ResponseAnswer_Answer_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "Answer",
                        principalColumn: "AnswerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponseAnswer_Response_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "Response",
                        principalColumn: "ResponseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResponseAnswer_AnswerId",
                table: "ResponseAnswer",
                column: "AnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response",
                column: "AnswerId",
                principalTable: "Answer",
                principalColumn: "AnswerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response");

            migrationBuilder.DropTable(
                name: "ResponseAnswer");

            migrationBuilder.AlterColumn<string>(
                name: "TextResponse",
                table: "Response",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ConnectionId",
                table: "Participant",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Response_Answer_AnswerId",
                table: "Response",
                column: "AnswerId",
                principalTable: "Answer",
                principalColumn: "AnswerId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
