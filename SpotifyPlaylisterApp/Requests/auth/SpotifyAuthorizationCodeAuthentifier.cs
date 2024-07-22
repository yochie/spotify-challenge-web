using System.Net;
using Newtonsoft.Json.Linq;
namespace SpotifyPlaylisterApp;

public class SpotifyAuthorizationCodeAuthentifier : IAuthenticationProvider
{
    public static readonly string httpClientName = "AuthorizationCodeClient";
    // dont change state (base url, etc) for this singleton since that can have impacts on other clients
    // kind of dumb that there is no way to prevent access to state, but what are ya gonna do... singletons
    // potential solutin would be to wrap in proxy that provides only access to request method or use DI along with IHttpClientFactory
    private IHttpClientFactory httpClientFactory;

    private readonly Uri endPoint;
    private Dictionary<string, string> formData = new();
    private AuthenticationToken? token;

    public SpotifyAuthorizationCodeAuthentifier(string endpoint,
                                                string clientID,
                                                string secret,
                                                string redirect_uri,
                                                IHttpClientFactory httpClientFactory)
    {
        endPoint = new Uri(endpoint);
        formData["grant_type"] = "client_credentials";
        formData["response_type"] = "code";
        formData["redirect_uri"] = redirect_uri;
        //TODO: enable state tracking for more security
        //see https://datatracker.ietf.org/doc/html/rfc6749#section-4.1
        //formData["state"] = ...
        formData["scope"] = "playlist-read-private playlist-read-collaborative";
        formData["client_id"] = clientID;
        formData["client_secret"] = secret;
        formData["show_dialog"] = "false";
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
        using HttpClient httpClient = httpClientFactory.CreateClient(SpotifyAuthorizationCodeAuthentifier.httpClientName);
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