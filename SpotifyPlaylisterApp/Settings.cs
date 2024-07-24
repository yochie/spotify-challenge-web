// See https://aka.ms/new-console-template for more information
internal sealed class Settings {
    public required string AuthTokenEndPoint { get; set; }
    public required string AuthCodeEndpoint { get; set; }
    public required string ClientID { get; set;}
    public required string Secret { get; set;}
    public required string DataAPIAddress { get; set;}
    public required string RedirectUri {get; set;}
}