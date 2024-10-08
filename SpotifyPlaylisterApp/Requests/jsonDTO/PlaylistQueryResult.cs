// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

internal class PlaylistQueryResult
{
    [JsonProperty("id")]
    required internal string Id {get; set;}
    
    [JsonProperty("name")]
    required internal string Name {get; set;}

    [JsonProperty("owner")]
    required internal OwnerQueryResult Owner {get; set;}
}
