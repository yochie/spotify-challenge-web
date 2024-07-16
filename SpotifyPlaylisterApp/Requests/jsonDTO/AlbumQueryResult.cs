// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

public class AlbumQueryResult
{
    [JsonProperty("name")]
    required public string Name {get; set;}
}
