namespace IronDB.Core.Json.Parsing;

public interface IJsonParser : IDisposable
{
    bool Read();

    void ValidateFloat();

    void ValidatePool();

    string GenerateErrorState();

    OnStringReadDelegate OnStringRead { set; }

    public delegate void OnStringReadDelegate(UnmanagedWriteBuffer buffer, bool partial);
}
