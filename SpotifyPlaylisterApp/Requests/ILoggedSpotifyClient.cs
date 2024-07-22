namespace SpotifyPlaylisterApp.Requests
{
    public interface ILoggedSpotifyClient : ISpotifyClient
    {
        List<string> GetUserPlaylistIds();
    }
}