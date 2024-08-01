using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class userTokensExpirationTyped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Expiration",
                table: "AspNetUsers",
                newName: "SpotifyAccessTokenExpiration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpotifyAccessTokenExpiration",
                table: "AspNetUsers",
                newName: "Expiration");
        }
    }
}
