// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;

namespace SpotifyPlaylisterApp.Requests
{
    public class PlaylistParser : IJsonParser<PlaylistData>
    {
        public PlaylistData Parse(string data)
        { 
            if (data == "{}" || data == "")
                throw new Exception("Spotify returned empty response without error code... Thanks spotify.");
            List<TrackData> trackData = [];
            var playlist = JsonConvert.DeserializeObject<PlaylistQueryResult>(data);
            if (playlist == null || playlist.Owner == null)
                throw new Exception("Couldn't understand Spotify response. Could be that api changed...");

            PlaylistData playlistData = new(playlist.Id,
                                            playlist.Name,
                                            playlist.Owner.Name ?? playlist.Owner.Id);
            return playlistData;
        }
    }


    public record PlaylistData(string Id, string Name, string OwnerName){
        public List<TrackData> Tracks {get; set;} = [];
    }

}