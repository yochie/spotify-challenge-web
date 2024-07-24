using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotifyPlaylisterApp.Requests
{
    public interface ILoggedSpotifyClient : ISpotifyClient
    {
        bool IsAuthenticated();
        PageResult Authenticate();
        Task<List<string>> GetUserPlaylistIdsAsync(HttpResponse? response);
    }
}