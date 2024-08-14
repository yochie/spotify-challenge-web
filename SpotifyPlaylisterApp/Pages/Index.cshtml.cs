using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;
using SpotifyPlaylisterApp.Requests;
using SQLitePCL;

namespace SpotifyPlaylisterApp.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IAnonymousSpotifyClient spotify;

    public IndexModel(ILogger<IndexModel> logger, IAnonymousSpotifyClient spotify)
    {
        _logger = logger;
        this.spotify = spotify;
    }

    public PlaylistData? Playlist {get; set;}
    public string? Error {get; set;}
    public string? RawJsonResponse {get; set;}

    [BindProperty]
    public string? PlaylistId {get; set;}

    public async Task<ActionResult> OnPostAsync(){
        if(PlaylistId is null){
            return Page();
        }

        try {
            this.Playlist = await spotify.GetPlaylist(PlaylistId);
        } catch(Exception e){
            Error = e.Message;
        }
        return Page();
    }
}
