//using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SpotifyPlaylisterApp.Areas.Identity.Data;

namespace SpotifyPlaylisterApp.Models;

public class Playlist
{
    public int Id { get; set; }

    [DisplayName("Playlist ID")]
    public string SpotifyId { get; set; } = "";

    [DisplayName("Playlist Name")]
    public string Name { get; set; } = "";
    public string SpotifyOwnerName { get; set; } = "";
    public virtual ICollection<PlaylistTrack> Tracks { get; } = new List<PlaylistTrack>();

    //link to user account on site
    public List<SpotifyPlaylisterUser> SpotifyPlaylisterUsers {get;  set;} = [];
}
