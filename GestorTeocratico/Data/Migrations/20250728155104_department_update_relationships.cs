using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorTeocratico.Data.Migrations
{
    /// <inheritdoc />
    public partial class department_update_relationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Publishers_ResponsiblePublisherId",
                table: "Departments");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentId",
                table: "Responsibilities",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Publishers_ResponsiblePublisherId",
                table: "Departments",
                column: "ResponsiblePublisherId",
                principalTable: "Publishers",
                principalColumn: "PublisherId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Publishers_ResponsiblePublisherId",
                table: "Departments");

            migrationBuilder.AlterColumn<Guid>(
                name: "DepartmentId",
                table: "Responsibilities",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Publishers_ResponsiblePublisherId",
                table: "Departments",
                column: "ResponsiblePublisherId",
                principalTable: "Publishers",
                principalColumn: "PublisherId");
        }
    }
}
