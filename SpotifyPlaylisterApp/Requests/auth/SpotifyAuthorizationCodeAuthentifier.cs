using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Web;
using Newtonsoft.Json.Linq;
using Azure;
using System.Linq;
using SpotifyPlaylisterApp.Requests.auth;
namespace SpotifyPlaylisterApp;

public class SpotifyAuthorizationCodeAuthentifier : IAuthenticationProvider
{
    public static readonly string httpClientName = "AuthorizationCodeClient";
    private readonly Uri tokenEndpoint;

    // dont change state (base url, etc) for this singleton since that can have impacts on other clients
    // kind of dumb that there is no way to prevent access to state, but what are ya gonna do... singletons
    // potential solutin would be to wrap in proxy that provides only access to request method or use DI along with IHttpClientFactory
    private IHttpClientFactory httpClientFactory;

    private readonly Uri codeEndPoint;
    private readonly Dictionary<string, string?> authCodeQuery = new();
    private readonly Dictionary<string, string?> accessTokenPostParams = new();
    private readonly Dictionary<string, string?> accessTokenPostHeaders = new();
    private AuthenticationToken? token;

    public SpotifyAuthorizationCodeAuthentifier(Uri codeEndpoint,
                                                Uri tokenEndpoint,
                                                string clientID,
                                                string secret,
                                                string redirect_uri,
                                                IHttpClientFactory httpClientFactory)
    {
        this.codeEndPoint = codeEndpoint;
        this.tokenEndpoint = tokenEndpoint;

        //Auth code query params used for spotify login
        this.authCodeQuery["client_id"] = clientID;
        this.authCodeQuery["response_type"] = "code";
        this.authCodeQuery["redirect_uri"] = redirect_uri;
        //TODO: enable state tracking for more security
        //see https://datatracker.ietf.org/doc/html/rfc6749#section-4.1
        //formData["state"] = ...
        this.authCodeQuery["scope"] = "playlist-read-private playlist-read-collaborative";
        this.authCodeQuery["show_dialog"] = "false";

        //Access token post params/headers
        this.accessTokenPostParams["grant_type"] = "authorization_code";
        //not actually redirected, just for validation
        this.accessTokenPostParams["redirect_uri"] = redirect_uri;
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{clientID}:{secret}");
        var encodedClientInfo = System.Convert.ToBase64String(bytes);
        this.accessTokenPostHeaders["Authorization"] = $"Basic {encodedClientInfo}";
        this.accessTokenPostHeaders["Content-Type"] = "application/x-www-form-urlencoded";

        this.httpClientFactory = httpClientFactory;
    }


    //returns valid access token
    //will fetch new one if none gotten yet or expired
    public async Task<string> GetAccessToken(HttpResponse? httpResponse){
        if(httpResponse is null)
            throw new ArgumentException("Cant request AccessToken in auth code workflow without providing response for redirection.");
        if (token == null){
            RedirectToRequestCode(httpResponse);
            return "";
        } else if (token.Expired){
            token = await this.RequestNewAuthToken();
        }
        return token.AccessToken;
    }

    private void RedirectToRequestCode(HttpResponse httpResponse){
        var uriBuilder = new UriBuilder(this.codeEndPoint);
        var qs = QueryString.Create(this.authCodeQuery);
        uriBuilder.Query = qs.ToString();
        httpResponse.Redirect(uriBuilder.ToString(), false);
    }

    private async Task<AuthenticationToken> RequestNewAuthToken()
    {
        //TODO add refresh capacity
        using HttpClient httpClient = httpClientFactory.CreateClient(SpotifyAuthorizationCodeAuthentifier.httpClientName);
        HttpResponseMessage response = await httpClient.PostAsync(codeEndPoint, new FormUrlEncodedContent(authCodeQuery));
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