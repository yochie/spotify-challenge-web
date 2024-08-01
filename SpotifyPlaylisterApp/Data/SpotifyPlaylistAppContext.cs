using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Data
{
    public class SpotifyPlaylisterAppContext : IdentityDbContext<SpotifyPlaylisterUser>
    {
        public SpotifyPlaylisterAppContext(DbContextOptions<SpotifyPlaylisterAppContext> options)
            : base(options)
        {
        }

        public DbSet<SpotifyUser> SpotifyUser { get; set; } = default!;
        public DbSet<Playlist> Playlist { get; set; } = default!;
        public DbSet<PlaylistTrack> PlaylistTrack { get; set; } = default!;
    }
}
