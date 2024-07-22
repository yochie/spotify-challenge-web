// See https://aka.ms/new-console-template for more information
internal sealed class Settings {
    public required string AuthAPIAddress { get; set; }
    public required string AuthCodeAddress { get; set; }
    public required string ClientID { get; set;}
    public required string Secret { get; set;}
    public required string DataAPIAddress { get; set;}
    public required string RedirectUri {get; set;}
}