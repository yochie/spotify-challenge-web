//using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpotifyPlaylisterApp.Models;

public class PlaylistCreateVM
{
    public int Id { get; set; }

    [DisplayName("Playlist Name")]
    public required string Name { get; set; }

    [DisplayName("Username")]
    public required int SpotifyUserId { get; set; } 
}
