using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelsWithFluentAPI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notebooks",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId1",
                table: "Notebooks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CreatedAt",
                table: "Notes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_OwnerId1",
                table: "Notebooks",
                column: "OwnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notebooks_Title",
                table: "Notebooks",
                column: "Title");

            migrationBuilder.AddForeignKey(
                name: "FK_Notebooks_AspNetUsers_OwnerId1",
                table: "Notebooks",
                column: "OwnerId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notebooks_AspNetUsers_OwnerId1",
                table: "Notebooks");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CreatedAt",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notebooks_OwnerId1",
                table: "Notebooks");

            migrationBuilder.DropIndex(
                name: "IX_Notebooks_Title",
                table: "Notebooks");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "Notebooks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notebooks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
