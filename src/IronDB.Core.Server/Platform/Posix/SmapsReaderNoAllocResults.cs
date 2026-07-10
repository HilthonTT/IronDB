namespace IronDB.Core.Server.Platform.Posix;

internal readonly struct SmapsReaderNoAllocResults : ISmapsReaderResultAction
{
    public void Add(SmapsReaderResults results)
    {
        // currently we do not use these results with SmapsReaderNoAllocResults so we do not store them
    }
}