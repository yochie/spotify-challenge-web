using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.Playlists
{
    public class IndexModel : PageModel
    {
        private readonly SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext _context;

        public IndexModel(SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext context)
        {
            _context = context;
        }

        public IList<Playlist> Playlist { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Playlist = await _context.Playlist
                .Include(p => p.SpotifyUser).ToListAsync();
        }
    }
}
