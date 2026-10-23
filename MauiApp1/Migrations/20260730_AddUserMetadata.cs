using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MauiApp1.Migrations
{
    [DbContext(typeof(MauiApp1.Data.MatiriParishDbContext))]
    public partial class AddUserMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add per-user hashing metadata
            migrationBuilder.AddColumn<int>(
                name: "Iterations",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHashAlgorithm",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            // Add lockout fields
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedLoginUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Iterations",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHashAlgorithm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedLoginCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastFailedLoginUtc",
                table: "Users");
        }
    }
}
