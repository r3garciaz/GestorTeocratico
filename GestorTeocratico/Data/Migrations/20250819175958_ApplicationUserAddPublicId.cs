using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorTeocratico.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserAddPublicId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers",
                column: "PublicId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PublicId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AspNetUsers");
        }
    }
}
