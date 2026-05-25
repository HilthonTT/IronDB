namespace IronDB.StorageEngine.Debugging;

public sealed class EnvironmentStorageReport
{
    public string BasePath { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public StorageReport Report { get; set; } = default!;
}
