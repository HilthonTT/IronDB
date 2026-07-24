using IronDB.Core.Server.Unmanaged;
using System.Collections;
using System.Drawing;

namespace IronDB.StorageEngine.Data.Fixed;

public unsafe partial class FixedSizeTree<TVal>
{
    public interface IFixedSizeIterator : IDisposable
    {
        bool SeekToLast();
        bool Seek(TVal key);
        bool SeekBackward(TVal key);
        TVal CurrentKey { get; }
        byte* ValuePtr(out int valueSize);
        ByteStringContext.Scope Value(out Slice slice);
        bool MoveNext();
        bool MovePrev();

        ValueReader CreateReaderForCurrent();

        bool Skip(long count);
    }

    public sealed class NullIterator : IFixedSizeIterator
    {
        public static readonly NullIterator Instance = new();
        public bool SeekToLast()
        {
            return false;
        }

        public bool Seek(TVal key)
        {
            return false;
        }

        public bool SeekBackward(TVal key)
        {
            return false;
        }

        public TVal CurrentKey { get { throw new InvalidOperationException("Invalid position, cannot read past end of tree"); } }
        public Slice Value { get { throw new InvalidOperationException("Invalid position, cannot read past end of tree"); } }

        ByteStringContext.Scope IFixedSizeIterator.Value(out Slice slice)
        {
            slice = new Slice();
            return new ByteStringContext<ByteStringMemoryCache>.Scope();
        }

        byte* IFixedSizeIterator.ValuePtr(out int valueSize)
        {
            valueSize = 0;
            return null;
        }

        public bool MoveNext()
        {
            return false;
        }

        public bool MovePrev()
        {
            return false;
        }

        public void Dispose()
        {
        }

        public ValueReader CreateReaderForCurrent()
        {
            throw new InvalidOperationException("No current page");
        }

        public bool Skip(long count)
        {
            return false;
        }
    }

    public sealed class EmbeddedIterator : IFixedSizeIterator
    {
        private readonly FixedSizeTree<TVal> _fst;
        private readonly ByteStringContext _allocator;
        private long _pos;
        private readonly FixedSizeTreeHeader.Embedded* _header;
        private readonly byte* _dataStart;
        private readonly int _changesAtStart;

        public EmbeddedIterator(FixedSizeTree<TVal> fst)
        {
            _fst = fst;
            _allocator = fst._tx.Allocator;
            _changesAtStart = _fst._changes;
            var ptr = _fst._parent.DirectRead(_fst._treeName);
            _header = (FixedSizeTreeHeader.Embedded*)ptr;
            _dataStart = ptr + sizeof(FixedSizeTreeHeader.Embedded);
        }

        public bool SeekToLast()
        {
            if (_header is null)
            {
                return false;
            }

            _pos = _header->NumberOfEntries - 1;

            return true;
        }

        public bool Seek(TVal key)
        {
            if (_header is null)
            {
                return false;
            }

            _pos = _fst.BinarySearch(_dataStart, _header->NumberOfEntries, key, _fst._entrySize);

            if (_fst._lastMatch > 0)
            {
                _pos++; // We didn't find the key.
            }
            return _pos != _header->NumberOfEntries;
        }

        public bool SeekBackward(TVal key)
        {
            if (_header is null)
            {
                return false;
            }

            _pos = _fst.BinarySearch(_dataStart, _header->NumberOfEntries, key, _fst._entrySize);

            if (_fst._lastMatch < 0)
            {
                _pos--; // We didn't find the key.
            }

            return _pos != -1;
        }

        public TVal CurrentKey
        {
            get
            {
                if (_pos >= _header->NumberOfEntries)
                {
                    throw new InvalidOperationException("Invalid position, cannot read past end of tree");
                }
                return FixedSizeTreePage<TVal>.GetEntry(_dataStart, (int)_pos, _fst._entrySize)->GetKey<TVal>();
            }
        }

        public long NumberOfEntriesLeft
        {
            get
            {
                if (_fst._lastMatch == 0)
                {
                    return _header->NumberOfEntries - _pos - 1;
                }

                return _header->NumberOfEntries - _pos;
            }
        }

        public ByteStringContext.Scope Value(out Slice slice)
        {
            if (_pos == _header->NumberOfEntries)
            {
                throw new InvalidOperationException("Invalid position, cannot read past end of tree");
            }

            return Slice.External(_allocator, _dataStart + (_pos * _fst._entrySize) + sizeof(long), _fst._valSize, out slice);
        }

        public byte* ValuePtr(out int valueSize)
        {
            if (_pos == _header->NumberOfEntries)
            {
                throw new InvalidOperationException("Invalid position, cannot read past end of tree");
            }

            valueSize = _fst._valSize;
            return _dataStart + (_pos * _fst._entrySize) + sizeof(long);
        }

        public bool MovePrev()
        {
            AssertNoChanges();
            return --_pos >= 0;
        }

        private void AssertNoChanges()
        {
            if (_changesAtStart != _fst._changes)
            {
                throw new InvalidOperationException("You cannot perform modifications to tree when iterator is opened.");
            }
        }

        public bool MoveNext()
        {
            AssertNoChanges();
            return ++_pos < _header->NumberOfEntries;
        }

        public ValueReader CreateReaderForCurrent()
        {
            return new ValueReader(_dataStart + (_pos * _fst._entrySize) + sizeof(long), _fst._valSize);
        }

        public bool Skip(long count)
        {
            if (count != 0)
            {
                _pos += count;
            }

            if (_pos < 0)
            {
                return false;
            }

            return _pos < _header->NumberOfEntries;
        }

        public void Dispose()
        {
        }
    }

    public sealed class LargeIterator : IFixedSizeIterator
    {
        private readonly FixedSizeTree<TVal> _parent;
        private readonly ByteStringContext _allocator;
        private readonly bool _prefetch;
        private FixedSizeTreePage<TVal>? _currentPage;
        private int _changesAtStart;

        public LargeIterator(FixedSizeTree<TVal> parent, bool prefetch = false)
        {
            _parent = parent;
            _allocator = parent._tx.Allocator;
            _changesAtStart = _parent._changes;
            _prefetch = prefetch;
        }

        private void AssertNoChanges()
        {
            if (_changesAtStart != _parent._changes)
            {
                throw new InvalidOperationException("You cannot perform modifications to tree when iterator is opened.");
            }
        }

        public void Dispose()
        {

        }

        public bool Seek(TVal key)
        {
            _currentPage = _parent.FindPageFor(key);
            return _currentPage.LastMatch <= 0 || MoveNext();
        }

        public bool SeekBackward(TVal key)
        {
            _currentPage = _parent.FindPageFor(key);
            return _currentPage.LastMatch >= 0 || MovePrev();
        }

        public bool SeekToLast()
        {
            _currentPage = _parent.FindPageFor(TVal.MaxValue);
            return true;
        }

        public TVal CurrentKey
        {
            get
            {
                if (_currentPage is null)
                {
                    throw new InvalidOperationException("No current page was set");
                }

                return _currentPage.GetKey(_currentPage.LastSearchPosition);
            }
        }

        public ByteStringContext.Scope Value(out Slice slice)
        {
            if (_currentPage is null)
            {
                throw new InvalidOperationException("No current page was set");
            }

            return Slice.External(_allocator,
                _currentPage.Pointer + _currentPage.StartPosition +
                (_parent._entrySize * _currentPage.LastSearchPosition) + sizeof(long), _parent._valSize,
                out slice);
        }

        public byte* ValuePtr(out int valueSize)
        {
            if (_currentPage is null)
            {
                throw new InvalidOperationException("No current page was set");
            }

            valueSize = _parent._valSize;
            return _currentPage.Pointer + _currentPage.StartPosition + (_parent._entrySize * _currentPage.LastSearchPosition) + sizeof(long);
        }


        private struct TreePagePrefetcher : IEnumerator<long>
        {
            private readonly FixedSizeTreePage<TVal> _page;
            private int _idx;

            public TreePagePrefetcher(FixedSizeTreePage<TVal> page)
            {
                System.Diagnostics.Debug.Assert(page.IsBranch);

                _page = page;
                _idx = -1;
            }

            readonly long IEnumerator<long>.Current => _page.GetEntry(_idx)->PageNumber;

            readonly object IEnumerator.Current => _page.GetEntry(_idx)->PageNumber;

            readonly void IDisposable.Dispose() { }

            bool IEnumerator.MoveNext()
            {
                _idx++;
                return _idx < _page.NumberOfEntries;
            }

            void IEnumerator.Reset()
            {
                _idx = -1;
            }
        }

        public bool MoveNext()
        {
            AssertNoChanges();

            while (_currentPage is not null)
            {
                _currentPage.LastSearchPosition++;
                if (_currentPage.LastSearchPosition < _currentPage.NumberOfEntries)
                {
                    // run out of entries, need to select the next page...
                    while (_currentPage.IsBranch)
                    {
                        _parent._cursor?.Push(_currentPage);

                        if (_prefetch)
                        {
                            var pager = _parent._tx.DataPager;
                            pager.MaybePrefetchMemory(new TreePagePrefetcher(_currentPage));
                        }

                        var childParentNumber = _currentPage.GetEntry(_currentPage.LastSearchPosition)->PageNumber;
                        _currentPage = _parent.GetReadOnlyPage(childParentNumber);

                        _currentPage.LastSearchPosition = 0;
                    }
                    return true;// there is another entry in this page
                }
                if (_parent._cursor?.Count == 0)
                {
                    break;
                }
                _currentPage = _parent._cursor?.Pop();
            }
            _currentPage = null;

            return false;
        }

        private bool MovePrev(long skip)
        {
            AssertNoChanges();

            if (_currentPage is null || !_currentPage.IsLeaf)
                throw new InvalidOperationException("No current page was set or it wasn't a leaf!");

            var currentPageHeader = _currentPage.PageHeader;
            int lastSearchPosition = _currentPage.LastSearchPosition;

            while (skip >= 0)
            {
                var skipInPage = (int)Math.Min(lastSearchPosition, skip);
                skip -= skipInPage;
                lastSearchPosition -= skipInPage;

                if (skip == 0)
                {
                    if (_currentPage.PageNumber != currentPageHeader.PageNumber)
                    {
                        _currentPage = _parent.GetReadOnlyPage(currentPageHeader.PageNumber);
                    }

                    _currentPage.LastSearchPosition = lastSearchPosition;
                    return true;
                }

                // Need to move to previous page
                while (true)
                {
                    if (_parent._cursor?.TryPeek(out var parent) != true)
                        return false;

                    parent.LastSearchPosition--;

                    if (parent.LastSearchPosition < 0)
                    {
                        _parent._cursor.Pop();
                        continue;
                    }

                    var nextChildPageNumber = parent.GetEntry(parent.LastSearchPosition)->PageNumber;
                    var childPageHeader = _parent.GetPageHeader(nextChildPageNumber);

                    if ((childPageHeader.TreeFlags & FixedSizeTreePageFlags.Branch) == FixedSizeTreePageFlags.Branch)
                    {
                        var childPage = _parent.GetReadOnlyPage(nextChildPageNumber);
                        childPage.LastSearchPosition = childPage.NumberOfEntries; // start from end
                        _parent._cursor.Push(childPage);
                        continue;
                    }

                    // Found previous leaf page
                    currentPageHeader = childPageHeader;
                    lastSearchPosition = childPageHeader.NumberOfEntries;
                    break;
                }
            }

            _currentPage = null;
            return false;
        }

        private bool MoveNext(long skip)
        {
            AssertNoChanges();

            if (_currentPage is null || !_currentPage.IsLeaf)
                throw new InvalidOperationException("No current page was set or it wasn't a leaf!");

            var currentPageHeader = _currentPage.PageHeader;
            int lastSearchPosition = _currentPage.LastSearchPosition;

            while (skip >= 0)
            {
                var skipInPage = (int)Math.Min(currentPageHeader.NumberOfEntries - lastSearchPosition, skip);
                skip -= skipInPage;
                lastSearchPosition += skipInPage;

                if (skip == 0)
                {
                    if (_currentPage.PageNumber != currentPageHeader.PageNumber)
                    {
                        _currentPage = _parent.GetReadOnlyPage(currentPageHeader.PageNumber);
                    }

                    _currentPage.LastSearchPosition = lastSearchPosition;

                    // If we landed exactly at or past the end → go to next page
                    if (_currentPage.LastSearchPosition >= _currentPage.NumberOfEntries)
                        return MoveNext(0); // recursive call with skip=0 to continue

                    return true;
                }

                // Need to move to the next page (climb up the tree)
                while (true)
                {
                    if (_parent._cursor?.TryPeek(out var parent) != true)
                        return false;

                    parent.LastSearchPosition++;

                    if (parent.LastSearchPosition >= parent.NumberOfEntries)
                    {
                        _parent._cursor.Pop();
                        continue;
                    }

                    var nextChildPageNumber = parent.GetEntry(parent.LastSearchPosition)->PageNumber;
                    var childPageHeader = _parent.GetPageHeader(nextChildPageNumber);

                    if ((childPageHeader.TreeFlags & FixedSizeTreePageFlags.Branch) == FixedSizeTreePageFlags.Branch)
                    {
                        var childPage = _parent.GetReadOnlyPage(nextChildPageNumber);
                        childPage.LastSearchPosition = -1; // will be incremented to 0 on next iteration
                        _parent._cursor.Push(childPage);
                        continue;
                    }

                    // Found next leaf page
                    currentPageHeader = childPageHeader;
                    lastSearchPosition = 0;
                    break;
                }
            }

            _currentPage = null;
            return false;
        }

        public bool MovePrev()
        {
            AssertNoChanges();

            if (_currentPage is null)
            {
                throw new InvalidOperationException("No current page was set");
            }

            while (_currentPage is not null)
            {
                _currentPage.LastSearchPosition--;
                if (_currentPage.LastSearchPosition >= 0)
                {
                    // run out of entries, need to select the next page...
                    while (_currentPage.IsBranch)
                    {
                        _parent._cursor?.Push(_currentPage);
                        var childParentNumber = _currentPage.GetEntry(_currentPage.LastSearchPosition)->PageNumber;
                        _currentPage = _parent.GetReadOnlyPage(childParentNumber);

                        _currentPage.LastSearchPosition = _currentPage.NumberOfEntries - 1;
                    }
                    return true;// there is another entry in this page
                }
                if (_parent._cursor?.Count == 0)
                {
                    break;
                }
                _currentPage = _parent._cursor?.Pop();
            }
            _currentPage = null;

            return false;
        }


        public ValueReader CreateReaderForCurrent()
        {
            if (_currentPage is null)
            {
                throw new InvalidOperationException("No current page was set");
            }

            return new ValueReader(_currentPage.Pointer + _currentPage.StartPosition + (_parent._entrySize * _currentPage.LastSearchPosition) + sizeof(long), _parent._valSize);
        }

        public bool Skip(long count)
        {
            if (count > 0)
            {
                if (!MoveNext(count))
                {
                    return false;
                }
            }
            else if (count < 0)
            {
                if (!MovePrev(-count))
                {
                    return false;
                }
            }

            bool seek = _currentPage is not null && _currentPage.LastSearchPosition != _currentPage.NumberOfEntries;
            if (!seek)
            {
                _currentPage = null;
            }
            return seek;
        }
    }
}
