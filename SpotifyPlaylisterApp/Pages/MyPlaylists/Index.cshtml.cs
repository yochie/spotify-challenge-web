using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
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
            string playlistIdsJson = await _spotify.GetUserPlaylistIdsAsync(HttpContext);

            List<string> freshPlaylistIds = _playlistIdParser.Parse(playlistIdsJson).List;

            var user = await _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User)) ?? throw new UnauthorizedAccessException();
            List<string> dbPlaylistIds = user.Playlists.Select(p => p.SpotifyId).ToList();
            //foreach followed playlist, update tracks
            await UpdateUserPlaylists(freshPlaylistIds, dbPlaylistIds);

            return Page();
        }

        private async Task UpdateUserPlaylists(List<string> freshPlaylistIds, List<string> dbPlaylistIds)
        {
            //create missing playlists and update existing ones
            foreach(string playlistId in freshPlaylistIds){

                string RawJsonResponse = "";      
                RawJsonResponse = await _spotify.GetPlaylist(playlistId, Response);
                var PlaylistData = _parser.Parse(RawJsonResponse);

                //find corresponding playlist in db
                var dbPlaylist = await _context.Playlist.FirstOrDefaultAsync(p => p.SpotifyId == playlistId);

                if (dbPlaylist is not null){
                    UpdatePlaylist(dbPlaylist, PlaylistData);
                } else {
                    CreatePlaylist(PlaylistData);
                }
            }
            //delete removed playlists
            foreach(string dbPlaylistId in dbPlaylistIds){
                if(!freshPlaylistIds.Contains(dbPlaylistId)){
                    var toRemove = await _context.Playlist.FirstAsync(p => p.SpotifyId == dbPlaylistId);
                    _context.Remove(toRemove);
                }
            }
            await _context.SaveChangesAsync();
        }

        private void UpdatePlaylist(Playlist dbPlaylist, PlaylistData freshPlaylistData)
        {
            dbPlaylist.Name = freshPlaylistData.Name;
            dbPlaylist.SpotifyOwnerName = freshPlaylistData.OwnerName;
            foreach(TrackData freshTrack in freshPlaylistData.Tracks){
                var dbTrack = dbPlaylist.Tracks.FirstOrDefault(dbTrack => dbTrack.SpotifyId == freshTrack.SpotifyId);
                if(dbTrack is not null){
                   dbTrack.Title = freshTrack.Name;
                   dbTrack.Album = freshTrack.Album;
                   dbTrack.Artists = freshTrack.Artists;
                } else {
                    CreateTrack(dbPlaylist, freshTrack);
                }
            }

            foreach(PlaylistTrack dbTrack in dbPlaylist.Tracks){
                var stillExists = freshPlaylistData.Tracks.Any(freshTrack => freshTrack.SpotifyId == dbTrack.SpotifyId);
                if(!stillExists){
                    _context.Remove(dbTrack);
                }
            }
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
                CreateTrack(playlist, trackData);
            }
        }

        private void CreateTrack(Playlist playlist, TrackData trackData){
            var track = new PlaylistTrack{
                SpotifyId = trackData.SpotifyId,
                Playlist = playlist,
                Title = trackData.Name,
                Album = trackData.Album,
                Artists = trackData.Artists
            };

            _context.Add(track);
        }
    }

}
