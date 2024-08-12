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
        private readonly string[] scopes;
        private readonly Uri _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonParser<PlaylistIdList> playlistIdParser;
        private readonly IJsonParser<PlaylistData> playlistContentParser;
        private readonly string _userId;

        public LoggedSpotifyClient(IHttpContextAccessor http,
                                   SpotifyPlaylisterAppContext context,
                                   IAuthenticationProvider authenticationProvider,
                                   string endpointUri,
                                   string[] scopes,
                                   IHttpClientFactory httpClient,
                                   UserManager<SpotifyPlaylisterUser> userManager,
                                   IJsonParser<PlaylistIdList> playlistIdParser,
                                   IJsonParser<PlaylistData> playlistContentParser)
        {
            _http = http;
            _context = context;
            _authentifier = authenticationProvider;
            this.scopes = scopes;
            _endpoint = new Uri(endpointUri);
            _httpClientFactory = httpClient;
            this.playlistIdParser = playlistIdParser;
            this.playlistContentParser = playlistContentParser;
            _userId = userManager.GetUserId(_http.HttpContext!.User) ?? throw new UnauthorizedAccessException();
        }

        public async Task<bool> IsAuthorized()
        {
            var user = await _context.Users.FirstAsync(u => u.Id == _userId);
            return user.SpotifyAccessToken is not null;
        }

        public async Task<string> GetPlaylist(string id, HttpResponse? response)
        {
            string accessToken = await _authentifier.GetAccessToken();
            using HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);
            string fieldQuery = "fields=id,name,owner.id,owner.display_name,tracks.items(track(id,name,artists(name),album(name)))";
            var msg = new HttpRequestMessage();
            msg.Headers.Add("Authorization", "Bearer " + accessToken);
            msg.Method = HttpMethod.Get;
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"playlists/{id}";
            uriBuilder.Query = fieldQuery;
            msg.RequestUri = uriBuilder.Uri;
            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            var apiResponse = await httpClient.SendAsync(msg);

            if (apiResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Couldn't request playlist data. Q: {msg.RequestUri}\n status : {apiResponse.StatusCode}");
            }
            return await apiResponse.Content.ReadAsStringAsync();
        }

        public async Task<List<string>> GetUserPlaylistIdsAsync()
        {
            
            string accessToken;
            accessToken = await _authentifier.GetAccessToken();
            using HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);
            string fieldQuery = "limit=50";
            var msg = new HttpRequestMessage();
            msg.Headers.Add("Authorization", "Bearer " + accessToken);
            msg.Method = HttpMethod.Get;
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"me/playlists";
            uriBuilder.Query = fieldQuery;
            msg.RequestUri = uriBuilder.Uri;
            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            List<string> playlistIdList = new();
            PlaylistIdList queryResult;
            do {
                var apiResponse = await httpClient.SendAsync(msg);

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Couldn't request playlist data. Error Code : {apiResponse.StatusCode}");
                }
                var json = await apiResponse.Content.ReadAsStringAsync();
                queryResult = playlistIdParser.Parse(json);
                playlistIdList.AddRange(queryResult.List);
                if(queryResult.NextPage != ""){
                    msg.RequestUri = new Uri(queryResult.NextPage);
                }
            } while(queryResult.NextPage != "");
            return playlistIdList;
        }

        public async Task Challenge(HttpContext httpContext)
        {
            var props = new AuthenticationProperties();
            props.SetParameter<string>("scope", scopes.Aggregate("", (str, next) => str + " " + next));
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