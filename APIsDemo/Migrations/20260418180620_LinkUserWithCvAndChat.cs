using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APIsDemo.Migrations
{
    /// <inheritdoc />
    public partial class LinkUserWithCvAndChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Cvs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Conversations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Cvs_UserId1",
                table: "Cvs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cvs_Users_UserId1",
                table: "Cvs",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Cvs_Users_UserId1",
                table: "Cvs");

            migrationBuilder.DropIndex(
                name: "IX_Cvs_UserId1",
                table: "Cvs");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Cvs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Conversations");
        }
    }
}
