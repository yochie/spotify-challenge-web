// See https://aka.ms/new-console-template for more information
internal interface IAuthenticationProvider
{
    public Task<string> GetAccessToken();
}
