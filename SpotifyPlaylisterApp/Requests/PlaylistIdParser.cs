using System.Linq;
using Newtonsoft.Json;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Pages.MyPlaylists;
namespace SpotifyPlaylisterApp.Requests {
    internal class PlaylistIdParser : IJsonParser<PlaylistIdList>
    {
        public PlaylistIdList Parse(string json)
        {
            List<string> ids = [];
            var playlistQuery = JsonConvert.DeserializeObject<UserPlaylistsQueryResult>(json);
            if (playlistQuery == null || playlistQuery.Items == null)
                throw new Exception("couldnt parse json");

            return new PlaylistIdList(playlistQuery.Items.Select(i => i.PlaylistId).ToList());
        }

    }

    public record PlaylistIdList(List<string> List);
}