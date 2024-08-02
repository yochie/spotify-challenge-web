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
    private readonly SpotifyPlaylisterAppContext _context;
    private readonly IAnonymousSpotifyClient spotify;
    private readonly IJsonParser<PlaylistData> parser;

    public IndexModel(ILogger<IndexModel> logger,
                      SpotifyPlaylisterAppContext context,
                      IAnonymousSpotifyClient spotify,
                      IJsonParser<PlaylistData> parser)
    {
        _logger = logger;
        _context = context;
        this.spotify = spotify;
        this.parser = parser;
    }

    public void OnGet()
    {
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
            RawJsonResponse = await spotify.GetPlaylist(PlaylistId);
        } catch(Exception e){
            RawJsonResponse = $"Error : couldn't find playlist";
            Error = "Couldn't find requested playlist.";
            _logger.LogError(e.ToString());
            return Page();
        }
        try {
            this.Playlist = parser.Parse(RawJsonResponse);
        } catch {
            Error = "Problem parsing results.";
        }
        return Page();
    }
}
