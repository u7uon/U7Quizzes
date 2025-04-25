using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace U7Quizzes.Migrations
{
    /// <inheritdoc />
    public partial class Implement_SoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Tag",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tag",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Session",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Session",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Response",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Response",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Quiz",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Quiz",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Question",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Question",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Participant",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Participant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Category",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Category",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Answer",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Answer",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Response");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Response");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Quiz");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Participant");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Participant");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Answer");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Answer");
        }
    }
}
