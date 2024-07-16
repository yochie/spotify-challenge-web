using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SpotifyPlaylistApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.Playlists
{
    public class CreateModelVM : PageModel
    {
        private readonly SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext _context;

        public CreateModelVM(SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["SpotifyUserId"] = new SelectList(
                _context.SpotifyUser.AsNoTracking(), 
                nameof(SpotifyUser.Id), 
                nameof(SpotifyUser.Name));
            return Page();
        }

        [BindProperty]
        public PlaylistCreateVM PlaylistVM { get; set; } = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var entry = _context.Add(new Playlist());
            entry.CurrentValues.SetValues(PlaylistVM);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
