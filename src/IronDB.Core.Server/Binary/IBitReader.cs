namespace IronDB.Core.Server.Binary;

public interface IBitReader
{
    int Length { get; }
    
    Bit Read();
}