using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using Polly;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Requests;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace SpotifyPlaylisterApp.Pages.Callbacks
{
    public class IndexModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;
        private readonly UserManager<SpotifyPlaylisterUser> _userManager;
        private readonly ILoggedSpotifyClient _spotify;

        public IndexModel(SpotifyPlaylisterAppContext context, UserManager<SpotifyPlaylisterUser> userManager, ILoggedSpotifyClient spotify)
        {
            _context = context;
            _userManager = userManager;
            _spotify = spotify;
        }

        public string? AuthResult {get; set;}

        public async Task<IActionResult> OnGetAsync()
        {
            AuthResult = "Spotify authorized. Return to my plalists.";
            try {
                await _spotify.HandleAuthorizationCallback(HttpContext); 
            } catch {
                AuthResult = "Error authorizing spotify...";
            }
            return Page();
        }
    }
}