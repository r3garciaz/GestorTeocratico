using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorTeocratico.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMeetingTypeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingSchedules_MeetingTypes_MeetingTypeId",
                table: "MeetingSchedules");

            migrationBuilder.DropTable(
                name: "MeetingTypes");

            migrationBuilder.DropIndex(
                name: "IX_MeetingSchedules_MeetingTypeId",
                table: "MeetingSchedules");

            migrationBuilder.DropColumn(
                name: "MeetingTypeId",
                table: "MeetingSchedules");

            migrationBuilder.AddColumn<string>(
                name: "MeetingType",
                table: "MeetingSchedules",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MeetingType",
                table: "MeetingSchedules");

            migrationBuilder.AddColumn<Guid>(
                name: "MeetingTypeId",
                table: "MeetingSchedules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MeetingTypes",
                columns: table => new
                {
                    MeetingTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingTypes", x => x.MeetingTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSchedules_MeetingTypeId",
                table: "MeetingSchedules",
                column: "MeetingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingSchedules_MeetingTypes_MeetingTypeId",
                table: "MeetingSchedules",
                column: "MeetingTypeId",
                principalTable: "MeetingTypes",
                principalColumn: "MeetingTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
