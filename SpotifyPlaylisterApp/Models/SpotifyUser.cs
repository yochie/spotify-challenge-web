using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpotifyPlaylisterApp.Models;

public class SpotifyUser
{
    public int Id { get; set; }

    [DisplayName("Spotify Username")]
    public string Name { get; set; } = null!;
    public virtual ICollection<Playlist> Playlists{ get; } = new List<Playlist>();
}