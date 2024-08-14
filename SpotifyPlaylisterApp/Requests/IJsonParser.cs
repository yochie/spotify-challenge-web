// See https://aka.ms/new-console-template for more information
namespace SpotifyPlaylisterApp.Requests;

public interface IJsonParser<RecordT>
{
    public RecordT Parse(string json);
}