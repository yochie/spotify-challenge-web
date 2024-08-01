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
    public class IndexModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;

        public IndexModel(SpotifyPlaylisterAppContext context)
        {
            _context = context;
        }

        public IList<SpotifyUser> SpotifyUser { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SpotifyUser = await _context.SpotifyUser.ToListAsync();
        }
    }
}
