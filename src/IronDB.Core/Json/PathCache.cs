using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace IronDB.Core.Json;

internal sealed class PathCache
{
    private int _used = -1;
    private readonly PathCacheHolder[] _items = new PathCacheHolder[512];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AcquirePathCache(
        out Dictionary<StringSegment, object> pathCache,
        out Dictionary<int, object> pathCacheByIndex)
    {
        // PERF: Avoids allocating gigabytes in FastDictionnary instances on high traffic RW operations like indexing

        if (_used >= 0)
        {
            var cache = _items[_used--];
            Debug.Assert(cache.Path is not null);
            Debug.Assert(cache.ByIndex is not null);

            pathCache = cache.Path;
            pathCacheByIndex = cache.ByIndex;

            return;
        }

        pathCache = new Dictionary<StringSegment, object>(StringSegmentEqualityStructComparer.BoxedInstance);
        pathCacheByIndex = [];
    }

    public void ReleasePathCache(
        Dictionary<StringSegment, object> pathCache,
        Dictionary<int, object> pathCacheByIndex)
    {
        if (_used >= _items.Length - 1 || pathCache.Count >= 256)
        {
            return;
        }

        pathCache.Clear();
        pathCacheByIndex.Clear();

        _items[++_used] = new PathCacheHolder
        {
            Path = pathCache,
            ByIndex = pathCacheByIndex
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearUnreturnedPathCache()
    {
        for (var i = _used + 1; i < _items.Length; i++)
        {
            var cache = _items[i];

            //never allocated, no reason to continue seeking
            if (cache.Path is null)
            {
                break;
            }

            //idly there shouldn't be unreleased path cache but we do have placed where we don't dispose of blittable object readers
            //and rely on the context.Reset to clear unwanted memory, but it didn't take care of the path cache.

            //Clear references for allocated cache paths so the GC can collect them.
            cache.ByIndex.Clear();
            cache.Path.Clear();
        }
    }

    private struct PathCacheHolder
    {
        public Dictionary<StringSegment, object> Path;
        public Dictionary<int, object> ByIndex;
    }
}
