using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyPlaylisterApp.Requests
{
    public interface ILoggedSpotifyClient : ISpotifyClient
    {
        Task<bool> IsAuthorized();
        Task<string> GetUserPlaylistIdsAsync();
        Task Challenge(HttpContext httpContext);
        Task HandleAuthorizationCallback(HttpContext httpContext);
    }
}