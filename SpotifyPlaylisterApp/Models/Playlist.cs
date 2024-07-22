//using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SpotifyPlaylisterApp.Areas.Identity.Data;

namespace SpotifyPlaylisterApp.Models;

public class Playlist
{
    public int Id { get; set; }

    [DisplayName("Playlist Name")]
    public string Name { get; set; } = "";
    public int SpotifyUserId { get; set; }
    public virtual SpotifyUser? SpotifyUser { get; set; }
    public virtual ICollection<PlaylistTrack> Tracks { get; } = new List<PlaylistTrack>();
    public List<SpotifyPlaylisterUser> SpotifyPlaylisterUsers {get;  set;} = [];
}
