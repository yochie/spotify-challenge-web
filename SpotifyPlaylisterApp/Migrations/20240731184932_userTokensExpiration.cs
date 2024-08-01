using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class userTokensExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Expiration",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "AspNetUsers");
        }
    }
}
