using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SpotifyPlaylisterApp.Areas.Identity.Data;

namespace SpotifyPlaylisterApp.Models;

public class UpdateProgress
{
    public int Id { get; set; }
    public float Progress {get; set;} = 0f;
    public bool Done {get; set;} = false;
    public SpotifyPlaylisterUser SpotifyPlaylisterUser {get;  set;} = null!;
}
