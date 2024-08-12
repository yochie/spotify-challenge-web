using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylisterApp.Authorization;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;

namespace SpotifyPlaylisterApp.Pages.Playlists
{
    public class DetailsModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;
        private readonly IAuthorizationService _authorizationService;

        public DetailsModel(SpotifyPlaylisterAppContext context, IAuthorizationService  authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        public Playlist Playlist { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var playlist = await _context.Playlist.Include(p => p.Tracks).Include(p => p.SpotifyPlaylisterUsers).FirstOrDefaultAsync(m => m.Id == id);
            if (playlist == null)
            {
                return NotFound();
            }
            var authoriztionResult = await _authorizationService.AuthorizeAsync(User, playlist, new PlaylistOwnerRequirement());
            if(!authoriztionResult.Succeeded){
                return Forbid();
            }
            Playlist = playlist;
            return Page();
        }
    }
}
