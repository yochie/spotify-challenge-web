using System.Net;
using Newtonsoft.Json.Linq;
namespace SpotifyPlaylisterApp;

public class SpotifyClientCredentialAuthentifier : IAuthenticationProvider
{
    public static readonly string httpClientName = "clientCredetialClient";
    private IHttpClientFactory httpClientFactory;

    private readonly Uri endPoint;
    private Dictionary<string, string> formData = new();
    private AuthenticationToken? token;

    public SpotifyClientCredentialAuthentifier(string endpoint, string clientID, string secret, IHttpClientFactory httpClientFactory)
    {
        endPoint = new Uri(endpoint);
        formData["grant_type"] = "client_credentials";
        formData["client_id"] = clientID;
        formData["client_secret"] = secret;
        this.token = null;
        this.httpClientFactory = httpClientFactory;
    }


    //returns valid access token
    //will fetch new one if none gotten yet or expired
    public async Task<string> GetAccessToken(){

        if (token == null || token.Expired){
            token = await RequestNewAuthToken();
        }
        return token.AccessToken;
    }

    private async Task<AuthenticationToken> RequestNewAuthToken()
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(SpotifyClientCredentialAuthentifier.httpClientName);
        HttpResponseMessage response = await httpClient.PostAsync(endPoint, new FormUrlEncodedContent(formData));
        if (response.StatusCode != HttpStatusCode.OK){
            throw new Exception($"Couldn't request authentication from API. Status : {response.StatusCode}");
        }
        var rawJsonResponse = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(rawJsonResponse);
        string? accessToken = (string?) json["access_token"];
        int? tokenSeconds = (int?) json["expires_in"];
        if (accessToken == null || tokenSeconds == null){
            throw new Exception("Couldn't parse authentication response");
        }
        return new AuthenticationToken(accessToken, (int)tokenSeconds);
    }
}