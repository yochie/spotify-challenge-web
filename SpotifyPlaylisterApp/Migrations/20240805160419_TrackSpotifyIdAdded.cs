using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class TrackSpotifyIdAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpotifyId",
                table: "PlaylistTrack",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpotifyId",
                table: "PlaylistTrack");
        }
    }
}
