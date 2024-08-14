using Newtonsoft.Json;

namespace SpotifyPlaylisterApp.Requests; 

public class PlaylistTracksParser : IJsonParser<PlaylistTracksData>
{
    public PlaylistTracksData Parse(string json)
    { 
        if (json == "{}" || json == "")
            throw new Exception("Spotify returned empty response without error code... Thanks spotify.");
        List<TrackData> trackData = [];
        var tracks = JsonConvert.DeserializeObject<TracksQueryResult>(json);
        if (tracks == null || tracks.Items == null)
            throw new Exception("Couldn't understand Spotify response. Could be that api changed...");
        foreach (var track in tracks.Items.Select(i => i.Track)) {
            string artists = track.Artists.Select(a => a.Name).Aggregate((a, b) => a + " / " + b);
            //for users own music, no ids are provided
            //combine the main track data in that case to make up an id
            //if there are multiple tracks with identical main track info, they will be merged in db
            //probably safer to ignore them...
            //todo : find better solution...
            string spotifyId = track.SpotifyId ?? track.Name + "%" + artists + "%" + track.Album.Name;
            trackData.Add(new TrackData(track.Name, track.Album.Name, artists, spotifyId));
        }
        return new PlaylistTracksData(trackData, tracks.NextPage ?? "");
    }
}
public record TrackData(string Name, string Album, string Artists, string SpotifyId);
public record PlaylistTracksData(List<TrackData> Tracks, string NextPage);