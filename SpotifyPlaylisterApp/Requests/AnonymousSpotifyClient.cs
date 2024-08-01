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
        private readonly IAuthenticationProvider authentifier;
        private readonly Uri endpoint;
        private readonly IHttpClientFactory httpClientFactory;

        public AnonymousSpotifyClient(IAuthenticationProvider authentifier, string endpointUri, IHttpClientFactory httpClient)
        {
            this.authentifier = authentifier;
            this.endpoint = new Uri(endpointUri);
            this.httpClientFactory = httpClient;
        }

        public async Task<string> GetPlaylist(string id, HttpResponse? response = null)
        {
            using HttpClient httpClient = httpClientFactory.CreateClient(LoggedSpotifyClient.httpClientName);
            string fieldQuery = "fields=name,owner.id,tracks.items(track(name,artists(name),album(name)))";
            string accessToken = await authentifier.GetAccessToken();
            var msg = new HttpRequestMessage();
            msg.Headers.Add("Authorization", "Bearer " + accessToken);
            msg.Method = HttpMethod.Get;
            UriBuilder uriBuilder = new(endpoint);
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
    }
}