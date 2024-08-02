using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class SpotifyUserDbRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Playlist_SpotifyUser_SpotifyUserId",
                table: "Playlist");

            migrationBuilder.DropTable(
                name: "SpotifyUser");

            migrationBuilder.DropIndex(
                name: "IX_Playlist_SpotifyUserId",
                table: "Playlist");

            migrationBuilder.DropColumn(
                name: "SpotifyUserId",
                table: "Playlist");

            migrationBuilder.AddColumn<string>(
                name: "SpotifyOwnerName",
                table: "Playlist",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpotifyOwnerName",
                table: "Playlist");

            migrationBuilder.AddColumn<int>(
                name: "SpotifyUserId",
                table: "Playlist",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SpotifyUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotifyUser", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Playlist_SpotifyUserId",
                table: "Playlist",
                column: "SpotifyUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Playlist_SpotifyUser_SpotifyUserId",
                table: "Playlist",
                column: "SpotifyUserId",
                principalTable: "SpotifyUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
