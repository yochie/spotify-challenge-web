namespace SpotifyPlaylisterApp;

public class AuthenticationToken
{
    public string AccessToken { get; }
    public TimeSpan Duration { get; } 
    private readonly DateTime createdAt = DateTime.Now;

    public AuthenticationToken(string accessToken, int expiresAfterSeconds)
    {
        this.AccessToken = accessToken;
        this.Duration = TimeSpan.FromSeconds(expiresAfterSeconds);
    }

    public bool Expired => DateTime.Now - createdAt > Duration;
}
