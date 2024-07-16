// See https://aka.ms/new-console-template for more information
using System.Net;

//Handles requests for playlists
//Returns raw Json
internal class SpotifyClient : ISpotifyClient
{
    //for use by http client factory
    public static readonly string httpClientName = "DataClient";
    private readonly IAuthenticationProvider authentifier;
    private readonly Uri endpoint;
    private readonly IHttpClientFactory httpClientFactory;

    public SpotifyClient(IAuthenticationProvider authentifier, string endpointUri, IHttpClientFactory httpClient)
    {
        this.authentifier = authentifier;
        this.endpoint = new Uri(endpointUri);
        this.httpClientFactory = httpClient;
    }

    public async Task<string> GetPlaylist(string id)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(SpotifyClient.httpClientName);
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
        var response = await httpClient.SendAsync(msg);

        if (response.StatusCode != HttpStatusCode.OK){
            throw new Exception($"Couldn't request playlist data. Q: {msg.RequestUri}\n status : {response.StatusCode}");
        }
        return await response.Content.ReadAsStringAsync();
        // JObject json = JObject.Parse(rawJsonResponse);
        // return json;
    }
}
