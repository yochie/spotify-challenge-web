// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

internal class TrackQueryResult
{
    [JsonProperty("name")]
    required public string Name { get; set; }

    [JsonProperty("album")]
    required public AlbumQueryResult Album { get; set; }

    [JsonProperty("artists")]
    required public List<ArtistQueryResult> Artists { get; set; }
}
