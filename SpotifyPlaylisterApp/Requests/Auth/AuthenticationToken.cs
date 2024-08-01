namespace SpotifyPlaylisterApp.Requests.Auth;

public class AuthenticationToken
{
    public string AccessToken { get; }
    public DateTime ExpiresAt { get; }

    public AuthenticationToken(string accessToken, int expiresAfterSeconds)
    {
        AccessToken = accessToken;
        ExpiresAt = DateTime.Now.AddSeconds(expiresAfterSeconds);
    }

    public AuthenticationToken(string accessToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
    }

    public bool Expired => DateTime.Now > ExpiresAt;
}
