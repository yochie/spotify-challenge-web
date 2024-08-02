// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;

namespace SpotifyPlaylisterApp.Requests {
    public class PlaylistParser : IJsonParser<PlaylistData>
    {
        PlaylistData IJsonParser<PlaylistData>.Parse(string data)
        { 
            List<TrackData> trackData = [];
            var playlist = JsonConvert.DeserializeObject<PlaylistQueryResult>(data);
            if (playlist == null || playlist.Owner == null)
                throw new Exception("couldnt parse json");

            foreach (var track in playlist.Tracks.Items.Select(i => i.Track)) {
                string artists = track.Artists.Select(a => a.Name).Aggregate((a, b) => a + " / " + b);
                trackData.Add(new TrackData(track.Name, track.Album.Name, artists));
            }
            PlaylistData playlistData = new(playlist.Id,
                                            playlist.Name,
                                            playlist.Owner.Name ?? playlist.Owner.Id,
                                            trackData);
            return playlistData;
        }
    }

    public record TrackData(string Name, string Album, string Artists);
    public record PlaylistData(string Id, string Name, string OwnerName, IEnumerable<TrackData> Tracks);
}