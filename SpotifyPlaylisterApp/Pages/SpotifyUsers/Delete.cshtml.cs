using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.SpotifyUsers
{
    public class DeleteModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public DeleteModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SpotifyUser SpotifyUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotifyuser = await _context.SpotifyUser.FirstOrDefaultAsync(m => m.Id == id);

            if (spotifyuser == null)
            {
                return NotFound();
            }
            else
            {
                SpotifyUser = spotifyuser;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spotifyuser = await _context.SpotifyUser.FindAsync(id);
            if (spotifyuser != null)
            {
                SpotifyUser = spotifyuser;
                _context.SpotifyUser.Remove(SpotifyUser);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
