using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class PlaylistsToUsersRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaylistSpotifyPlaylisterUser",
                columns: table => new
                {
                    PlaylistsId = table.Column<int>(type: "INTEGER", nullable: false),
                    SpotifyPlaylisterUsersId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistSpotifyPlaylisterUser", x => new { x.PlaylistsId, x.SpotifyPlaylisterUsersId });
                    table.ForeignKey(
                        name: "FK_PlaylistSpotifyPlaylisterUser_AspNetUsers_SpotifyPlaylisterUsersId",
                        column: x => x.SpotifyPlaylisterUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistSpotifyPlaylisterUser_Playlist_PlaylistsId",
                        column: x => x.PlaylistsId,
                        principalTable: "Playlist",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSpotifyPlaylisterUser_SpotifyPlaylisterUsersId",
                table: "PlaylistSpotifyPlaylisterUser",
                column: "SpotifyPlaylisterUsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistSpotifyPlaylisterUser");
        }
    }
}
