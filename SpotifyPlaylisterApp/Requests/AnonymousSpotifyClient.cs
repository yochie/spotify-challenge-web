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
        private readonly IJsonParser<PlaylistTracksData> _playlistTracksParser;

        public AnonymousSpotifyClient(IAuthenticationProvider authentifier,
                                      string endpointUri,
                                      IHttpClientFactory httpClient,
                                      IJsonParser<PlaylistData> playlistParser,
                                      IJsonParser<PlaylistTracksData> playlistTracksParser)
        {
            this._authentifier = authentifier;
            this._endpoint = new Uri(endpointUri);
            this._httpClientFactory = httpClient;
            this._playlistParser = playlistParser;
            _playlistTracksParser = playlistTracksParser;
        }

        public async Task<PlaylistData> GetPlaylist(string id, HttpResponse? response = null)
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient(LoggedSpotifyClient.httpClientName);
            string fieldQuery = "fields=name,owner.id";
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