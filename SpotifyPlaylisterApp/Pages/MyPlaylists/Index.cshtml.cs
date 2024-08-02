using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Media;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Polly;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;
using SpotifyPlaylisterApp.Requests;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;


namespace SpotifyPlaylisterApp.Pages.MyPlaylists
{
    public class UserPlaylistsModel : PageModel
    {
        private readonly SpotifyPlaylisterAppContext _context;
        private readonly UserManager<SpotifyPlaylisterUser> _userManager;
        private readonly ILoggedSpotifyClient _spotify;
        private readonly IJsonParser<PlaylistData> _parser;
        private readonly IJsonParser<PlaylistIdList> _playlistIdParser;

        public UserPlaylistsModel(SpotifyPlaylisterAppContext context,
                                  UserManager<SpotifyPlaylisterUser> userManager,
                                  ILoggedSpotifyClient spotify,
                                  IJsonParser<PlaylistData> parser,
                                  IJsonParser<PlaylistIdList> playlistIdParser)
        {
            _context = context;
            _userManager = userManager;
            this._spotify = spotify;
            this._parser = parser;
            _playlistIdParser = playlistIdParser;
        }

        public List<Playlist> Playlists { get;set; } = [];

        public bool SpotifyAuthorized = false;

        public async Task OnGetAsync()
        {
            var user = await _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User)) ?? throw new UnauthorizedAccessException();
            Playlists = user.Playlists ?? [];
            SpotifyAuthorized = await _spotify.IsAuthorized();
        }

        public async Task<PageResult> OnPostAuth(){
            await _spotify.Challenge(HttpContext);
            return Page();
        }

        public async Task<PageResult> OnPostUpdate(){


            //Get list of playlists that user follows
            string playlistIds = await _spotify.GetUserPlaylistIdsAsync(HttpContext);

            List<string> parsed = _playlistIdParser.Parse(playlistIds).List;

            //foreach followed playlist, update tracks
            await UpdatePlaylists(parsed);

            return Page();
        }

        //TODO : complete code
        private async Task UpdatePlaylists(List<string> playlistIds)
        {
           // throw new NotImplementedException();
            foreach(string playlistId in playlistIds){

                string RawJsonResponse = "";      
                RawJsonResponse = await _spotify.GetPlaylist(playlistId, Response);
                var PlaylistData = _parser.Parse(RawJsonResponse);

                //find corresponding playlist in db
                var dbPlaylist = await _context.Playlist.FirstOrDefaultAsync(p => p.SpotifyId == playlistId);

                if (dbPlaylist is not null){
                    UpdatePlaylistTracks(dbPlaylist, PlaylistData);
                } else {
                    CreatePlaylist(PlaylistData);
                }
            }
            await _context.SaveChangesAsync();
        }

        private void UpdatePlaylistTracks(Playlist dbPlaylist, PlaylistData playlistData)
        {
            throw new NotImplementedException();
        }

        private async void CreatePlaylist(PlaylistData playlistData){
            var playlist = new Playlist
            {
                Name = playlistData.Name,
                SpotifyId = playlistData.Id,
                SpotifyOwnerName = playlistData.OwnerName
            };

            _context.Add(playlist);
            await RegisterToCurrentUser(playlist);
            CreatePlaylistTracks(playlist, playlistData.Tracks);
        }

        private async Task RegisterToCurrentUser(Playlist playlist)
        {
            var user = await _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User)) ?? throw new UnauthorizedAccessException();
            user.Playlists.Add(playlist); 
        }

        private void CreatePlaylistTracks(Playlist playlist, IEnumerable<TrackData> tracksData)
        {
            foreach(var trackData in tracksData){
                var track = new PlaylistTrack{
                    Playlist = playlist,
                    Title = trackData.Name,
                    Album = trackData.Album,
                    Artists = trackData.Artists
                };

                _context.Add(track);
            }
        }
    }

}
