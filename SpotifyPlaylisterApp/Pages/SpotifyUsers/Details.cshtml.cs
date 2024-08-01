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
    public class DetailsModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public DetailsModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        public SpotifyUser SpotifyUser { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id, string? penis)
        {
       
            if (id == null)
            {
                return NotFound();
            }

            var spotifyuser = await _context.SpotifyUser.Include(u => u.Playlists).FirstOrDefaultAsync(m => m.Id == id);
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
    }
}
