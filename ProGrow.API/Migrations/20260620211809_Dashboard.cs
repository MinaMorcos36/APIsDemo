using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProGrow.API.Migrations
{
    /// <inheritdoc />
    public partial class Dashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CVUrl",
                table: "UserProfiles",
                newName: "CVName");

            migrationBuilder.AddColumn<int>(
                name: "CVScore",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostMediaUrl",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JobCategoryId",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeclined",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "JobCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__JobCategories__3214EC07", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobCategoryId",
                table: "Jobs",
                column: "JobCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_JobCategories",
                table: "Jobs",
                column: "JobCategoryId",
                principalTable: "JobCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_JobCategories",
                table: "Jobs");

            migrationBuilder.DropTable(
                name: "JobCategories");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobCategoryId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CVScore",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PostMediaUrl",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "JobCategoryId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "IsDeclined",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "CVName",
                table: "UserProfiles",
                newName: "CVUrl");
        }
    }
}
