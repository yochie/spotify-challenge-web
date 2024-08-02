// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

public class UserPlaylistsItemsQueryResult
{

    [JsonProperty("id")]
    required public string PlaylistId { get; set; }
}