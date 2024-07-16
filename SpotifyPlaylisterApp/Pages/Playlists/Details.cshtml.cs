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
    public class DetailsModel : PageModel
    {
        private readonly SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext _context;

        public DetailsModel(SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext context)
        {
            _context = context;
        }

        public Playlist Playlist { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlist.Include(p => p.SpotifyUser).FirstOrDefaultAsync(m => m.Id == id);
            if (playlist == null)
            {
                return NotFound();
            }
            else
            {
                Playlist = playlist;
            }
            return Page();
        }
    }
}
