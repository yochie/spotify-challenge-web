// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

internal class OwnerQueryResult
{
    [JsonProperty("id")]
    required internal string Id {get; set;}

    [JsonProperty("display_name")]
    required internal string Name {get; set;}
}