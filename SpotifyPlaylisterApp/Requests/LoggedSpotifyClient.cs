// See https://aka.ms/new-console-template for more information
using System.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SpotifyPlaylisterApp;
using SpotifyPlaylisterApp.Requests.auth;

namespace SpotifyPlaylisterApp.Requests
{

    //Handles requests for playlists
    //Returns raw Json
    internal class LoggedSpotifyClient : ISpotifyClient, ILoggedSpotifyClient
    {
        //for use by http client factory
        public static readonly string httpClientName = "DataClient";
        private readonly IAuthenticationProvider authentifier;
        private readonly Uri endpoint;
        private readonly IHttpClientFactory httpClientFactory;

        public LoggedSpotifyClient(IAuthenticationProvider authenticationProvider, string endpointUri, IHttpClientFactory httpClient)
        {
            authentifier = authenticationProvider;
            endpoint = new Uri(endpointUri);
            httpClientFactory = httpClient;
        }

        public bool IsAuthenticated()
        {
            return false;
        }

        public PageResult Authenticate()
        {
            throw new NotImplementedException(); }

        public async Task<string> GetPlaylist(string id, HttpResponse? response)
        {
            string accessToken = await authentifier.GetAccessToken(response);
            using HttpClient httpClient = httpClientFactory.CreateClient(httpClientName);
            string fieldQuery = "fields=name,owner.id,tracks.items(track(name,artists(name),album(name)))";
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
            // JObject json = JObject.Parse(rawJsonResponse);
            // return json;
        }

        public async Task<List<string>> GetUserPlaylistIdsAsync(HttpResponse? response)
        {
            string accessToken = await authentifier.GetAccessToken(response);
            if (accessToken == "")
                return [];
            throw new NotImplementedException();
        }

    }
}