// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
namespace SpotifyPlaylisterApp;

public class UserPlaylistsQueryResult
{
    [JsonProperty("items")]
    required public List<UserPlaylistsItemsQueryResult> Items { get; set; }
}
