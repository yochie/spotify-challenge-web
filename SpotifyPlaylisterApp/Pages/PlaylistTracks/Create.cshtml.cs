using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.PlaylistTracks
{
    public class CreateModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public CreateModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["PlaylistId"] = new SelectList(_context.Playlist, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public PlaylistTrack PlaylistTrack { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.PlaylistTrack.Add(PlaylistTrack);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
