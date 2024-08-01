using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.Playlists
{
    public class DeleteModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public DeleteModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Playlist Playlist { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlist.FirstOrDefaultAsync(m => m.Id == id);

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playlist = await _context.Playlist.FindAsync(id);
            if (playlist != null)
            {
                Playlist = playlist;
                _context.Playlist.Remove(Playlist);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
