using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Migrations
{
    /// <inheritdoc />
    public partial class AddAgoraUidToParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_SpaceId",
                table: "Participants");

            migrationBuilder.AddColumn<long>(
                name: "AgoraUid",
                table: "Participants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SpaceId_AgoraUid",
                table: "Participants",
                columns: new[] { "SpaceId", "AgoraUid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_SpaceId_AgoraUid",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "AgoraUid",
                table: "Participants");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SpaceId",
                table: "Participants",
                column: "SpaceId");
        }
    }
}
