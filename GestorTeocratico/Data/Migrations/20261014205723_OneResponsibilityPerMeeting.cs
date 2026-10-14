using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorTeocratico.Data.Migrations
{
    /// <inheritdoc />
    public partial class OneResponsibilityPerMeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Keep a single assignment per (MeetingScheduleId, ResponsibilityId) before switching the
            // primary key. Duplicates are collapsed to the row with the smallest PublisherId.
            migrationBuilder.Sql(
                """
                DELETE FROM "ResponsibilityAssignments" a
                USING "ResponsibilityAssignments" b
                WHERE a."MeetingScheduleId" = b."MeetingScheduleId"
                  AND a."ResponsibilityId" = b."ResponsibilityId"
                  AND a."PublisherId" > b."PublisherId";
                """);

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponsibilityAssignments",
                table: "ResponsibilityAssignments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponsibilityAssignments",
                table: "ResponsibilityAssignments",
                columns: new[] { "MeetingScheduleId", "ResponsibilityId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ResponsibilityAssignments",
                table: "ResponsibilityAssignments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResponsibilityAssignments",
                table: "ResponsibilityAssignments",
                columns: new[] { "MeetingScheduleId", "ResponsibilityId", "PublisherId" });
        }
    }
}
