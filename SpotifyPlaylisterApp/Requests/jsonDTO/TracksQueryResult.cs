// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

internal class TracksQueryResult
{
    [JsonProperty("items")]
    required internal List<PlaylistTrackQueryResult> Items { get; set;}

    [JsonProperty("next")]
    required internal string? NextPage {get; set;}
}
