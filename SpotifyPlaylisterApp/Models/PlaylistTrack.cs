using System.ComponentModel.DataAnnotations;

namespace SpotifyPlaylisterApp.Models;

public class PlaylistTrack
{
    public int Id { get; set; }
    public string SpotifyId { get; set; } = "";
    public int Rating {get; set; }
    public string Title { get; set; } = "";
    public string Artists { get; set; } = "";
    public string Album { get; set; } = "";
    public int PlaylistId { get; set; } 
    public virtual Playlist? Playlist{ get; set; } = null!;
}
