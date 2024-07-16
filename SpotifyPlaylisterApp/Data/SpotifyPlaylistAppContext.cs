using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylistApp.Data
{
    public class SpotifyPlaylistAppContext : DbContext
    {
        public SpotifyPlaylistAppContext (DbContextOptions<SpotifyPlaylistAppContext> options)
            : base(options)
        {
        }

        public DbSet<SpotifyPlaylisterApp.Models.SpotifyUser> SpotifyUser { get; set; } = default!;
        public DbSet<SpotifyPlaylisterApp.Models.Playlist> Playlist { get; set; } = default!;
        public DbSet<SpotifyPlaylisterApp.Models.PlaylistTrack> PlaylistTrack { get; set; } = default!;
    }
}
