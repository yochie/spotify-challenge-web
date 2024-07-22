using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SpotifyPlaylistApp.Data;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Models;
using SpotifyPlaylisterApp.Requests;


namespace SpotifyPlaylisterApp.Pages.MyPlaylists
{
    public class UserPlaylistsModel : PageModel
    {
        private readonly SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext _context;
        private readonly UserManager<SpotifyPlaylisterUser> _userManager;
        private readonly ILoggedSpotifyClient _spotify;
        private readonly IJsonParser<PlaylistData> _parser;

        public UserPlaylistsModel(SpotifyPlaylistApp.Data.SpotifyPlaylistAppContext context,
                                  UserManager<SpotifyPlaylisterUser> userManager,
                                  ILoggedSpotifyClient spotify,
                                  IJsonParser<PlaylistData> parser)
        {
            _context = context;
            _userManager = userManager;
            this._spotify = spotify;
            this._parser = parser;
        }

        public List<Playlist> Playlists { get;set; } = [];

        public async Task OnGetAsync()
        {
            var user = await _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));
            if (user is not null){
                Playlists = user.Playlists ?? [];
            }
        }

        public async Task<PageResult> OnPostUpdate(){
            //Get list of playlists that user follows
            //TODO implement
            List<string> playlistIds = _spotify.GetUserPlaylistIds();

            //foreach followed playlist, update tracks
            await UpdatePlaylists(playlistIds);

            return Page();
        }

        //TODO : complete code
        private async Task UpdatePlaylists(List<string> playlistIds)
        {
           // throw new NotImplementedException();
            foreach(string playlistId in playlistIds){

                string RawJsonResponse = "";      
                RawJsonResponse = await _spotify.GetPlaylist(playlistId);
                var PlaylistData = _parser.Parse(RawJsonResponse);
                //find corresponding playlist in db

                //if it exists, update track list

                //otherwise, create it
                var entry = _context.Add(new Playlist());
                //entry.CurrentValues;
                await _context.SaveChangesAsync();
            }
        }
    }
}
