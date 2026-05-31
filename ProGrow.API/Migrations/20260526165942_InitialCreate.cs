using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProGrow.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Jobs");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true,
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<string>(
                name: "AboutRole",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BannerImageUrl",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CityOffice",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobType",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "(N'Full-time')");

            migrationBuilder.AddColumn<string>(
                name: "LocationMode",
                table: "Jobs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "(N'On-site')");

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Responsibilities",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryFrom",
                table: "Jobs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SalaryInInterview",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValueSql: "((0))");

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryTo",
                table: "Jobs",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "Jobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverLetter",
                table: "JobApplications",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvFileName",
                table: "JobApplications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CvFilePath",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CvId",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CvScore",
                table: "JobApplications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvScoreReason",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "JobApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioLink",
                table: "JobApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CvId",
                table: "Conversations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Comments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "JobId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobLikes__3214EC07", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobLikes_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobSaves__3214EC07", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobSaves_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JobSkill",
                columns: table => new
                {
                    JobId = table.Column<int>(type: "int", nullable: false),
                    SkillId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSkill", x => new { x.JobId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_JobSkill_Job",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobSkill_Skill",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_CvId",
                table: "JobApplications",
                column: "CvId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CvId",
                table: "Conversations",
                column: "CvId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_JobId",
                table: "Comments",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLikes_JobId",
                table: "JobLikes",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSaves_JobId",
                table: "JobSaves",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSkill_SkillId",
                table: "JobSkill",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK__Comment__JobId__79B8D9E4",
                table: "Comments",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Cvs_CvId",
                table: "Conversations",
                column: "CvId",
                principalTable: "Cvs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_Cvs_CvId",
                table: "JobApplications",
                column: "CvId",
                principalTable: "Cvs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__Comment__JobId__79B8D9E4",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Cvs_CvId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_Cvs_CvId",
                table: "JobApplications");

            migrationBuilder.DropTable(
                name: "JobLikes");

            migrationBuilder.DropTable(
                name: "JobSaves");

            migrationBuilder.DropTable(
                name: "JobSkill");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_CvId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CvId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Comments_JobId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AboutRole",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "BannerImageUrl",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CityOffice",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobType",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "LocationMode",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Responsibilities",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryFrom",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryInInterview",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "SalaryTo",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CoverLetter",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvFileName",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvFilePath",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvScore",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvScoreReason",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "PortfolioLink",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Comments");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Jobs",
                type: "bit",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Jobs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
