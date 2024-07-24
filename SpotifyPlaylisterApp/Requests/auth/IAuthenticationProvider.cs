// See https://aka.ms/new-console-template for more information
namespace SpotifyPlaylisterApp.Requests.auth
{
    public interface IAuthenticationProvider
    {
        public Task<string> GetAccessToken(HttpResponse? response = null);
    }
}