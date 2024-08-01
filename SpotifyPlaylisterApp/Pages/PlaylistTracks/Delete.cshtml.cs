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
    public class DeleteModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public DeleteModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlaylistTrack PlaylistTrack { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlisttrack = await _context.PlaylistTrack.FirstOrDefaultAsync(m => m.Id == id);

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlisttrack = await _context.PlaylistTrack.FindAsync(id);
            if (playlisttrack != null)
            {
                PlaylistTrack = playlisttrack;
                _context.PlaylistTrack.Remove(PlaylistTrack);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
