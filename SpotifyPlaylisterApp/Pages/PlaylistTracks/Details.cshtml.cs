using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.PlaylistTracks
{
    public class DetailsModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public DetailsModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        public PlaylistTrack PlaylistTrack { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlisttrack = await _context.PlaylistTrack.Include(t => t.Playlist).FirstOrDefaultAsync(m => m.Id == id);
            if (playlisttrack == null)
            {
                return NotFound();
            }
            else
            {
                PlaylistTrack = playlisttrack;
            }
            return Page();
        }
    }
}
