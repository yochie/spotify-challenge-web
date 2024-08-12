using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Media;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        public string Error {get; set;} = "";
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

        public async Task OnPostUpdate(){
            //Get list of playlists that user follows
            List<string> freshPlaylistIds;
            try {
                freshPlaylistIds = await _spotify.GetUserPlaylistIdsAsync();
            } catch (Exception e){
                Error = e.Message;
                return;
            }

            var user = await _context.Users
                .Include(u => u.Playlists)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User)) ?? throw new UnauthorizedAccessException();
            List<string> dbPlaylistIds = user.Playlists.Select(p => p.SpotifyId).ToList();
            //foreach followed playlist, update tracks
            await UpdateUserPlaylists(freshPlaylistIds, dbPlaylistIds);

            Response.Redirect(Request.GetEncodedUrl());
        }

        private async Task UpdateUserPlaylists(List<string> freshPlaylistIds, List<string> dbPlaylistIds)
        {
            //create missing playlists and update existing ones
            foreach(string playlistId in freshPlaylistIds){

                string RawJsonResponse = "";      
                RawJsonResponse = await _spotify.GetPlaylist(playlistId, Response);
                var PlaylistData = _parser.Parse(RawJsonResponse);

                //find corresponding playlist in db
                var dbPlaylist = await _context.Playlist.Include(p => p.Tracks).FirstOrDefaultAsync(p => p.SpotifyId == playlistId);

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

            List<PlaylistTrack> toAdd = new ();
            foreach(TrackData freshTrack in freshPlaylistData.Tracks){
                var dbTrack = dbPlaylist.Tracks.FirstOrDefault(dbTrack => dbTrack.SpotifyId == freshTrack.SpotifyId);
                if(dbTrack is not null){
                   dbTrack.Title = freshTrack.Name;
                   dbTrack.Album = freshTrack.Album;
                   dbTrack.Artists = freshTrack.Artists;
                } else {
                    var newTrack = CreateTrack(dbPlaylist, freshTrack);
                    if(newTrack is null || toAdd.Any(pt => pt.SpotifyId == newTrack.SpotifyId))
                        //skip duplicates
                        continue;
                    toAdd.Add(newTrack);
                }
            }
            _context.PlaylistTrack.AddRange(toAdd);

            foreach(PlaylistTrack dbTrack in dbPlaylist.Tracks){
                var stillExists = freshPlaylistData.Tracks.Any(freshTrack => freshTrack.SpotifyId == dbTrack.SpotifyId);
                if(!stillExists){
                    _context.PlaylistTrack.Remove(dbTrack);
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
            List<PlaylistTrack> toAdd = new();
            foreach(var trackData in tracksData){
                var newTrack = CreateTrack(playlist, trackData);
                if (newTrack is null || toAdd.Any(pt => pt.SpotifyId == newTrack.SpotifyId))
                    //ignore duplicates
                    continue;
                toAdd.Add(newTrack);
            }
            _context.PlaylistTrack.AddRange(toAdd);
        }

        //returns db object to create
        //callers should ensure the batch of created tracks contain no duplicates before saving
        private PlaylistTrack? CreateTrack(Playlist playlist, TrackData trackData){
            if(trackData.SpotifyId.IsNullOrEmpty())
                throw new Exception("trackData without id");
            if(_context.PlaylistTrack.Any(pt => pt.SpotifyId == trackData.SpotifyId)){
                //we will ignore duplicates by design here
                //we would need to track ordering otherwise
                //todo change if i want to add order tracking
                return null;
            }
            var track = new PlaylistTrack{
                SpotifyId = trackData.SpotifyId,
                Playlist = playlist,
                Title = trackData.Name,
                Album = trackData.Album,
                Artists = trackData.Artists
            };
            return track;
        }
    }

}
