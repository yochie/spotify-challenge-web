// See https://aka.ms/new-console-template for more information

namespace SpotifyPlaylisterApp.Requests
{
    public interface ISpotifyClient
    {
        public Task<PlaylistData> GetPlaylist(string playlistID, HttpResponse? response = null);
    }
}