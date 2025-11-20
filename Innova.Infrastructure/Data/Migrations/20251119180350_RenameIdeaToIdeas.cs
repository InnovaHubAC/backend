using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Innova.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdeaToIdeas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Idea_IdeaId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Idea_AspNetUsers_AppUserId",
                table: "Idea");

            migrationBuilder.DropForeignKey(
                name: "FK_Idea_Departments_DepartmentId",
                table: "Idea");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Idea",
                table: "Idea");

            migrationBuilder.RenameTable(
                name: "Idea",
                newName: "Ideas");

            migrationBuilder.RenameIndex(
                name: "IX_Idea_DepartmentId",
                table: "Ideas",
                newName: "IX_Ideas_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Idea_AppUserId",
                table: "Ideas",
                newName: "IX_Ideas_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ideas",
                table: "Ideas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Ideas_IdeaId",
                table: "Attachment",
                column: "IdeaId",
                principalTable: "Ideas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_AspNetUsers_AppUserId",
                table: "Ideas",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Ideas_IdeaId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_AspNetUsers_AppUserId",
                table: "Ideas");

            migrationBuilder.DropForeignKey(
                name: "FK_Ideas_Departments_DepartmentId",
                table: "Ideas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ideas",
                table: "Ideas");

            migrationBuilder.RenameTable(
                name: "Ideas",
                newName: "Idea");

            migrationBuilder.RenameIndex(
                name: "IX_Ideas_DepartmentId",
                table: "Idea",
                newName: "IX_Idea_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Ideas_AppUserId",
                table: "Idea",
                newName: "IX_Idea_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Idea",
                table: "Idea",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Idea_IdeaId",
                table: "Attachment",
                column: "IdeaId",
                principalTable: "Idea",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Idea_AspNetUsers_AppUserId",
                table: "Idea",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Idea_Departments_DepartmentId",
                table: "Idea",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
