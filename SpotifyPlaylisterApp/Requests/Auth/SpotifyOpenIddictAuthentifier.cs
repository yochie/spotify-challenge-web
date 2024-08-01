
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using SpotifyPlaylisterApp.Areas.Identity.Data;
using SpotifyPlaylisterApp.Data;
using SpotifyPlaylisterApp.Migrations;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace SpotifyPlaylisterApp.Requests.Auth;

public class SpotifyOpenIddictAuthentifier : IAuthenticationProvider {

    private AuthenticationToken? token = null;
    private readonly SpotifyPlaylisterAppContext _context;
    private readonly IHttpContextAccessor _http;
    private readonly string _userId;
    private readonly OpenIddictClientService _openIddictClient;

    public SpotifyOpenIddictAuthentifier(SpotifyPlaylisterAppContext context, IHttpContextAccessor http, OpenIddictClientService openIddictClient){
        this._context = context;
        this._http = http;
        this._openIddictClient = openIddictClient;
        this._userId = http.HttpContext!.User.GetClaim(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }

    public async Task<string> GetAccessToken()
    {

        token ??= await FromDb();

        if(token.Expired){
            await RefreshToken();
            token = await FromDb();
        }

        return token.AccessToken;
    }

    private async Task<AuthenticationToken> FromDb()
    {
        var user = await _context.Users.FirstAsync(u => u.Id == _userId);
        if(user.SpotifyAccessToken.IsNullOrEmpty() || user.SpotifyAccessTokenExpiration is null){
            throw new UnauthorizedAccessException("Only users that previously authorized spotify access should use this authentifier");
        }
        return new AuthenticationToken(user.SpotifyAccessToken!, user.SpotifyAccessTokenExpiration.Value);
    }

    private async Task RefreshToken(){
        var user = await _context.Users.FirstAsync(u => u.Id == _userId);
        var refreshToken = user.SpotifyRefreshToken ??
            throw new ArgumentNullException("No refresh token available");
        
        var result = await _openIddictClient.AuthenticateWithRefreshTokenAsync(new (){ RefreshToken = refreshToken});
        user.SpotifyRefreshToken = result.RefreshToken;
        user.SpotifyAccessToken = result.AccessToken;
        user.SpotifyAccessTokenExpiration = result.AccessTokenExpirationDate!.Value.DateTime;

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