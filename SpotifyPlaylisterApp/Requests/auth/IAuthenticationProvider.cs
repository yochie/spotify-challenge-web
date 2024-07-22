// See https://aka.ms/new-console-template for more information
public interface IAuthenticationProvider
{
    public Task<string> GetAccessToken();
}
