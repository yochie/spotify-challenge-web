// See https://aka.ms/new-console-template for more information
using System.Net;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Requests.Auth;

namespace SpotifyPlaylisterApp.Requests
{

    //Handles requests for playlists
    //Returns raw Json
    internal class AnonymousSpotifyClient : ISpotifyClient, IAnonymousSpotifyClient
    {
        //for use by http client factory
        public static readonly string httpClientName = "DataClient";
        private readonly IAuthenticationProvider _authentifier;
        private readonly Uri _endpoint;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJsonParser<PlaylistData> _playlistParser;

        public AnonymousSpotifyClient(IAuthenticationProvider authentifier,
                                      string endpointUri,
                                      IHttpClientFactory httpClient,
                                      IJsonParser<PlaylistData> playlistParser)
        {
            this._authentifier = authentifier;
            this._endpoint = new Uri(endpointUri);
            this._httpClientFactory = httpClient;
            this._playlistParser = playlistParser;
        }

        public async Task<PlaylistData> GetPlaylist(string id, HttpResponse? response = null)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient(LoggedSpotifyClient.httpClientName);
            string fieldQuery = "fields=name,owner.id,tracks.items(track(name,artists(name),album(name)))";
            UriBuilder uriBuilder = new(_endpoint);
            uriBuilder.Path += $"playlists/{id}";
            uriBuilder.Query = fieldQuery;
            string accessToken = await _authentifier.GetAccessToken();
            var msg = BuildMessage(uriBuilder.Uri, accessToken);
            PlaylistData playlistDataPage;
            PlaylistData? fullPlaylistData = null;
            do{
                var apiResponse = await httpClient.SendAsync(msg);

                if (apiResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Couldn't request playlist data. Q: {msg.RequestUri}\n status : {apiResponse.StatusCode}");
                }
                var json = await apiResponse.Content.ReadAsStringAsync();
                playlistDataPage = _playlistParser.Parse(json);
                if(fullPlaylistData == null){
                    fullPlaylistData = playlistDataPage;
                } else {
                    fullPlaylistData = fullPlaylistData with {Tracks = fullPlaylistData.Tracks.Concat(playlistDataPage.Tracks)};
                }
            } while(playlistDataPage.NextPage != "");
            return fullPlaylistData;
        }

        private HttpRequestMessage BuildMessage(Uri uri, string accessToken){
            var msg = new HttpRequestMessage();
            msg.Headers.Add("Authorization", "Bearer " + accessToken);
            msg.Method = HttpMethod.Get;
            msg.RequestUri = uri;
            msg.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return msg;
        }
    }
}