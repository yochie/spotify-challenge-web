// See https://aka.ms/new-console-template for more information
public interface IJsonParser<RecordT>
{
    public RecordT Parse(string data);
}