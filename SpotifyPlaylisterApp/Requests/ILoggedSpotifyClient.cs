using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyPlaylisterApp.Requests
{
    public interface ILoggedSpotifyClient : ISpotifyClient
    {
        Task<bool> IsAuthorized();
        Task<List<string>> GetUserPlaylistIdsAsync(HttpResponse? response);
        Task Challenge(HttpContext httpContext);
        Task HandleAuthorizationCallback(HttpContext httpContext);
    }
}