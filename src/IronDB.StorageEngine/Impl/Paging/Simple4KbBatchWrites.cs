using IronDB.Core;

namespace IronDB.StorageEngine.Impl.Paging;

public sealed unsafe class Simple4KbBatchWrites : I4KbBatchWrites
{
    private readonly AbstractPager _abstractPager;
    private PagerState? _pagerState;

    public Simple4KbBatchWrites(AbstractPager abstractPager)
    {
        _abstractPager = abstractPager;
        _pagerState = _abstractPager.GetPagerStateAndAddRefAtomically();
    }

    public void Write(long posBy4Kbs, int numberOf4Kbs, byte* source)
    {
        const int pageSizeTo4KbRatio = (Constants.Storage.PageSize / (4 * Constants.Size.Kilobyte));
        var pageNumber = posBy4Kbs / pageSizeTo4KbRatio;
        var offsetBy4Kb = posBy4Kbs % pageSizeTo4KbRatio;
        var numberOfPages = numberOf4Kbs / pageSizeTo4KbRatio;
        if (numberOf4Kbs % pageSizeTo4KbRatio != 0)
        {
            numberOfPages++;
        }

        var newPagerState = _abstractPager.EnsureContinuous(pageNumber, numberOfPages);
        if (newPagerState is not null)
        {
            _pagerState?.Release();
            newPagerState.AddRef();
            _pagerState = newPagerState;
        }

        var toWrite = numberOf4Kbs * 4 * Constants.Size.Kilobyte;
        byte* destination = _abstractPager.AcquirePagePointer(null, pageNumber, _pagerState)
                            + (offsetBy4Kb * 4 * Constants.Size.Kilobyte);

        _abstractPager.UnprotectPageRange(destination, (ulong)toWrite);

        Memory.Copy(destination, source, toWrite);

        _abstractPager.ProtectPageRange(destination, (ulong)toWrite);
    }

    public void Dispose()
    {
        _pagerState?.Release();
    }
}
