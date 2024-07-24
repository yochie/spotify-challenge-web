using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace SpotifyPlaylisterApp.Pages.Callbacks
{
    public class IndexModel : PageModel
    {
        private readonly SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext _context;

        public IndexModel(SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext context)
        {
            _context = context;
        }

        public string? AuthResult {get; set;}

        public async Task OnGetAsync()
        {
            var result = await HttpContext.AuthenticateAsync(Providers.Spotify);
            this.AuthResult = result!.Principal!.ToString();
        }
    }
}