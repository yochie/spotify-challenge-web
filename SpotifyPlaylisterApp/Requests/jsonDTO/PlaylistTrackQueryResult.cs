// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;


internal class PlaylistTrackQueryResult
{
    [JsonProperty("track")]
    required internal TrackQueryResult Track {get; set;}
}
