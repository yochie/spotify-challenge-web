using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyPlaylisterApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProgressAddedActually2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpdateProgress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Progress = table.Column<float>(type: "REAL", nullable: false),
                    Done = table.Column<bool>(type: "INTEGER", nullable: false),
                    SpotifyPlaylisterUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpdateProgress_AspNetUsers_SpotifyPlaylisterUserId",
                        column: x => x.SpotifyPlaylisterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpdateProgress_SpotifyPlaylisterUserId",
                table: "UpdateProgress",
                column: "SpotifyPlaylisterUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateProgress");
        }
    }
}
