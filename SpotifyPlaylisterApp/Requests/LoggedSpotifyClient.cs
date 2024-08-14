// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Models;
using SpotifyPlaylisterApp.Pages;
using SpotifyPlaylisterApp.Requests.Auth;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace SpotifyPlaylisterApp.Requests
{

    //Handles requests for playlists
    //Returns parsed results
    internal class LoggedSpotifyClient : ISpotifyClient, ILoggedSpotifyClient
    {
        //for use by http client factory
        public static readonly string httpClientName = "DataClient";
        private readonly IHttpContextAccessor _http;
        private readonly SpotifyPlaylisterAppContext _context;
        private readonly IAuthenticationProvider _authentifier;
        private readonly string[] _scopes;
        private readonly Uri _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonParser<PlaylistIdList> _playlistIdParser;
        private readonly IJsonParser<PlaylistData> _playlistParser;
        private readonly IJsonParser<PlaylistTracksData> _playlistTracksParser;
        private readonly string _userId;

        public LoggedSpotifyClient(IHttpContextAccessor http,
                                   SpotifyPlaylisterAppContext context,
                                   IAuthenticationProvider authenticationProvider,
                                   string endpointUri,
                                   string[] scopes,
                                   IHttpClientFactory httpClient,
                                   UserManager<SpotifyPlaylisterUser> userManager,
                                   IJsonParser<PlaylistIdList> playlistIdParser,
                                   IJsonParser<PlaylistData> playlistContentParser,
                                   IJsonParser<PlaylistTracksData> playlistTracksParser)
        {
            _http = http;
            _context = context;
            _authentifier = authenticationProvider;
            _scopes = scopes;
            _endpoint = new Uri(endpointUri);
            _httpClientFactory = httpClient;
            _playlistIdParser = playlistIdParser;
            _playlistParser = playlistContentParser;
            _playlistTracksParser = playlistTracksParser;
            _userId = userManager.GetUserId(_http.HttpContext!.User) ?? throw new UnauthorizedAccessException();
        }

        public async Task<bool> IsAuthorized()
        {
            var user = await _context.Users.FirstAsync(u => u.Id == _userId);
            return user.SpotifyAccessToken is not null;
        }

        public async Task<PlaylistData> GetPlaylist(string id, HttpResponse? response)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient(LoggedSpotifyClient.httpClientName);
            string fieldQuery = "fields=id,name,owner.id,owner.display_name";
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"playlists/{id}";
            uriBuilder.Query = fieldQuery;
            string accessToken = await _authentifier.GetAccessToken();
            var msg = BuildMessage(uriBuilder.Uri, accessToken);
            var apiResponse = await httpClient.SendAsync(msg);

            if (apiResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't request playlist tracks data. Error code : {apiResponse.StatusCode}");
            }
            var json = await apiResponse.Content.ReadAsStringAsync();
            var playlistData = _playlistParser.Parse(json);
            playlistData.Tracks = await GetPlaylistTracks(id);
            return playlistData;
        }

        //iterats on pages to return full list of tracks 
        private async Task<List<TrackData>> GetPlaylistTracks(string playlistId){
            using HttpClient httpClient = _httpClientFactory.CreateClient(LoggedSpotifyClient.httpClientName);
            string fieldQuery = "fields=items(track(id,name,artists(name),album(name))),next";
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"playlists/{playlistId}/tracks";
            uriBuilder.Query = fieldQuery;
            string accessToken = await _authentifier.GetAccessToken();
            var msg = BuildMessage(uriBuilder.Uri, accessToken);
            PlaylistTracksData tracksPage;
            List<TrackData>? fullTracks = null;
            do{
                var apiResponse = await httpClient.SendAsync(msg);

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Couldn't request playlist tracks data. Error code : {apiResponse.StatusCode}");
                }
                var json = await apiResponse.Content.ReadAsStringAsync();
                tracksPage = _playlistTracksParser.Parse(json);
                if(fullTracks == null){
                    fullTracks = tracksPage.Tracks;
                } else {
                    fullTracks.AddRange(tracksPage.Tracks);
                }
                if(tracksPage.NextPage != ""){
                    msg = BuildMessage(new Uri(tracksPage.NextPage), accessToken);
                }
            } while(tracksPage.NextPage != "");
            return fullTracks;
        }

        public async Task<List<string>> GetUserPlaylistIdsAsync()
        {
            string accessToken;
            accessToken = await _authentifier.GetAccessToken();
            using HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);
            string fieldQuery = "limit=50";
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"me/playlists";
            uriBuilder.Query = fieldQuery;
            var msg = BuildMessage(uriBuilder.Uri, accessToken);
            List<string> playlistIdList = new();
            PlaylistIdList queryResult;
            do {
                var apiResponse = await httpClient.SendAsync(msg);

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Couldn't request playlist data. Error Code : {apiResponse.StatusCode}");
                }
                var json = await apiResponse.Content.ReadAsStringAsync();
                queryResult = _playlistIdParser.Parse(json);
                playlistIdList.AddRange(queryResult.List);
                if(queryResult.NextPage != ""){
                    msg = BuildMessage(new Uri(queryResult.NextPage), accessToken);
                }
            } while(queryResult.NextPage != "");
            return playlistIdList;
        }
        
        private static HttpRequestMessage BuildMessage(Uri uri, string accessToken){
            var msg = new HttpRequestMessage();
            msg.Headers.Add("Authorization", "Bearer " + accessToken);
            msg.Method = HttpMethod.Get;
            msg.RequestUri = uri;
            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return msg;
        }

        public async Task Challenge(HttpContext httpContext)
        {
            var props = new AuthenticationProperties();
            props.SetParameter<string>("scope", _scopes.Aggregate("", (str, next) => str + " " + next));
            await Results.Challenge(props, authenticationSchemes: [Providers.Spotify]).ExecuteAsync(httpContext);
        }

        public async Task HandleAuthorizationCallback(HttpContext httpContext){
            var result = await httpContext.AuthenticateAsync(Providers.Spotify);
            if(result.Properties is null){
                throw new InvalidOperationException();
            }
            var properties = result.Properties;
            var tokens =  result.Properties.GetTokens();
            var user = await _context.Users.FirstAsync(u => u.Id == _userId)
                ?? throw new Exception("Couldn't identify current user.");
            var accessTokenName = OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken;
            var accessToken = result.Properties.GetTokenValue(accessTokenName);
            user.SpotifyAccessToken = accessToken;

            var expirationName = OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessTokenExpirationDate;
            var expiration = result.Properties.GetTokenValue(expirationName)
                ?? throw new Exception("access token expiration undefined");
            user.SpotifyAccessTokenExpiration = DateTime.Parse(expiration);

            var refreshTokenName = OpenIddictClientAspNetCoreConstants.Tokens.RefreshToken;
            var refreshToken = result.Properties.GetTokenValue(refreshTokenName);
            user.SpotifyRefreshToken = refreshToken;

             _context.Attach(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                    throw;
            }
        }
    }
}