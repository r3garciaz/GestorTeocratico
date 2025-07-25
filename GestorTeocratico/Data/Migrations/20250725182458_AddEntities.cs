using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorTeocratico.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Congregations",
                columns: table => new
                {
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    MidweekMeetingDayEvenYear = table.Column<string>(type: "text", nullable: false),
                    MidweekMeetingDayOddYear = table.Column<string>(type: "text", nullable: false),
                    WeekendMeetingDayEvenYear = table.Column<string>(type: "text", nullable: false),
                    WeekendMeetingDayOddYear = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Congregations", x => x.CongregationId);
                });

            migrationBuilder.CreateTable(
                name: "MeetingTypes",
                columns: table => new
                {
                    MeetingTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    TypeMeetingTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingTypes", x => x.MeetingTypeId);
                    table.ForeignKey(
                        name: "FK_MeetingTypes_MeetingTypes_TypeMeetingTypeId",
                        column: x => x.TypeMeetingTypeId,
                        principalTable: "MeetingTypes",
                        principalColumn: "MeetingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    LastName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    MotherLastName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Phone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Privilege = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.PublisherId);
                    table.ForeignKey(
                        name: "FK_Publishers_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "CongregationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingSchedules",
                columns: table => new
                {
                    MeetingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    WeekOfYear = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingSchedules", x => x.MeetingScheduleId);
                    table.ForeignKey(
                        name: "FK_MeetingSchedules_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "CongregationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingSchedules_MeetingTypes_MeetingTypeId",
                        column: x => x.MeetingTypeId,
                        principalTable: "MeetingTypes",
                        principalColumn: "MeetingTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CongregationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsiblePublisherId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.DepartmentId);
                    table.ForeignKey(
                        name: "FK_Departments_Congregations_CongregationId",
                        column: x => x.CongregationId,
                        principalTable: "Congregations",
                        principalColumn: "CongregationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Departments_Publishers_ResponsiblePublisherId",
                        column: x => x.ResponsiblePublisherId,
                        principalTable: "Publishers",
                        principalColumn: "PublisherId");
                });

            migrationBuilder.CreateTable(
                name: "Responsibilities",
                columns: table => new
                {
                    ResponsibilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responsibilities", x => x.ResponsibilityId);
                    table.ForeignKey(
                        name: "FK_Responsibilities_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublisherResponsibilities",
                columns: table => new
                {
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibilityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublisherResponsibilities", x => new { x.PublisherId, x.ResponsibilityId });
                    table.ForeignKey(
                        name: "FK_PublisherResponsibilities_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "PublisherId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublisherResponsibilities_Responsibilities_ResponsibilityId",
                        column: x => x.ResponsibilityId,
                        principalTable: "Responsibilities",
                        principalColumn: "ResponsibilityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityAssignmentConfigs",
                columns: table => new
                {
                    ResponsibilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityAssignmentConfigs", x => new { x.ResponsibilityId, x.MeetingTypeId });
                    table.ForeignKey(
                        name: "FK_ResponsibilityAssignmentConfigs_MeetingTypes_MeetingTypeId",
                        column: x => x.MeetingTypeId,
                        principalTable: "MeetingTypes",
                        principalColumn: "MeetingTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResponsibilityAssignmentConfigs_Responsibilities_Responsibi~",
                        column: x => x.ResponsibilityId,
                        principalTable: "Responsibilities",
                        principalColumn: "ResponsibilityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponsibilityAssignments",
                columns: table => new
                {
                    MeetingScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublisherId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsibilityAssignments", x => new { x.MeetingScheduleId, x.ResponsibilityId, x.PublisherId });
                    table.ForeignKey(
                        name: "FK_ResponsibilityAssignments_MeetingSchedules_MeetingScheduleId",
                        column: x => x.MeetingScheduleId,
                        principalTable: "MeetingSchedules",
                        principalColumn: "MeetingScheduleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityAssignments_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "PublisherId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ResponsibilityAssignments_Responsibilities_ResponsibilityId",
                        column: x => x.ResponsibilityId,
                        principalTable: "Responsibilities",
                        principalColumn: "ResponsibilityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CongregationId",
                table: "Departments",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ResponsiblePublisherId",
                table: "Departments",
                column: "ResponsiblePublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSchedules_CongregationId",
                table: "MeetingSchedules",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingSchedules_MeetingTypeId",
                table: "MeetingSchedules",
                column: "MeetingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTypes_TypeMeetingTypeId",
                table: "MeetingTypes",
                column: "TypeMeetingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublisherResponsibilities_ResponsibilityId",
                table: "PublisherResponsibilities",
                column: "ResponsibilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_CongregationId",
                table: "Publishers",
                column: "CongregationId");

            migrationBuilder.CreateIndex(
                name: "IX_Responsibilities_DepartmentId",
                table: "Responsibilities",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityAssignmentConfigs_MeetingTypeId",
                table: "ResponsibilityAssignmentConfigs",
                column: "MeetingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityAssignments_PublisherId",
                table: "ResponsibilityAssignments",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibilityAssignments_ResponsibilityId",
                table: "ResponsibilityAssignments",
                column: "ResponsibilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublisherResponsibilities");

            migrationBuilder.DropTable(
                name: "ResponsibilityAssignmentConfigs");

            migrationBuilder.DropTable(
                name: "ResponsibilityAssignments");

            migrationBuilder.DropTable(
                name: "MeetingSchedules");

            migrationBuilder.DropTable(
                name: "Responsibilities");

            migrationBuilder.DropTable(
                name: "MeetingTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Publishers");

            migrationBuilder.DropTable(
                name: "Congregations");
        }
    }
}
