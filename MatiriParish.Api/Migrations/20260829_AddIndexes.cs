using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatiriParish.Api.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Baptisms_PersonId",
                table: "Baptisms",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Confirmations_PersonId",
                table: "Confirmations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Marriages_GroomId",
                table: "Marriages",
                column: "GroomId");

            migrationBuilder.CreateIndex(
                name: "IX_Marriages_BrideId",
                table: "Marriages",
                column: "BrideId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Baptisms_PersonId",
                table: "Baptisms");

            migrationBuilder.DropIndex(
                name: "IX_Confirmations_PersonId",
                table: "Confirmations");

            migrationBuilder.DropIndex(
                name: "IX_Marriages_GroomId",
                table: "Marriages");

            migrationBuilder.DropIndex(
                name: "IX_Marriages_BrideId",
                table: "Marriages");
        }
    }
}
