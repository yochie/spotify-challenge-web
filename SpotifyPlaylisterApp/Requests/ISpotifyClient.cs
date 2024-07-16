// See https://aka.ms/new-console-template for more information
public interface ISpotifyClient
{
    public Task<string> GetPlaylist(string playlistID);
}
