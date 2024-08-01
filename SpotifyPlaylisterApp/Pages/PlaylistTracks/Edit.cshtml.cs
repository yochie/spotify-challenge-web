using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.PlaylistTracks
{
    public class EditModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public EditModel(SpotifyPlaylisterAppContext context)
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

            var playlisttrack =  await _context.PlaylistTrack.FirstOrDefaultAsync(m => m.Id == id);
            if (playlisttrack == null)
            {
                return NotFound();
            }
            PlaylistTrack = playlisttrack;
           ViewData["PlaylistId"] = new SelectList(_context.Playlist, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PlaylistTrack).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlaylistTrackExists(PlaylistTrack.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PlaylistTrackExists(int id)
        {
            return _context.PlaylistTrack.Any(e => e.Id == id);
        }
    }
}
