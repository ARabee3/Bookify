using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProgressSchemaFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Books_BookID",
                table: "Progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Chapters_LastReadChapterID",
                table: "Progresses");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_LastReadChapterID",
                table: "Progresses");

            migrationBuilder.RenameColumn(
                name: "LastReadChapterID",
                table: "Progresses",
                newName: "LastReadPageNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Books_BookID",
                table: "Progresses",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "BookID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Books_BookID",
                table: "Progresses");

            migrationBuilder.RenameColumn(
                name: "LastReadPageNumber",
                table: "Progresses",
                newName: "LastReadChapterID");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_LastReadChapterID",
                table: "Progresses",
                column: "LastReadChapterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Books_BookID",
                table: "Progresses",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "BookID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Chapters_LastReadChapterID",
                table: "Progresses",
                column: "LastReadChapterID",
                principalTable: "Chapters",
                principalColumn: "ChapterID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
