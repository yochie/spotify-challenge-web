// See https://aka.ms/new-console-template for more information
namespace SpotifyPlaylisterApp.Requests.Auth
{
    public interface IAuthenticationProvider
    {
        public Task<string> GetAccessToken();
    }
}