// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace IronDB.Core.Server.Utils;

// This class implements a text reader that reads from a string based on the CoreCLR version but allows to reset the position.
[ComVisible(true)]
public sealed class ReusableStringReader : TextReader
{
    private string? _s;
    private int _pos;
    private int _length;

    public int Length => _length;

    public ReusableStringReader(string s)
    {
        Contract.EndContractBlock();
        _s = s ?? throw new ArgumentNullException(nameof(s));
        _length = s.Length;
    }

    protected override void Dispose(bool disposing)
    {
        _s = null;
        _pos = 0;
        _length = 0;

        base.Dispose(disposing);
    }

    public void Reset()
    {
        _pos = 0;
    }

    // Returns the next available character without actually reading it from
    // the underlying string. The current position of the StringReader is not
    // changed by this operation. The returned value is -1 if no further
    // characters are available.
    //
    [Pure]
    public override int Peek()
    {
        if (_s is null)
        {
            ThrowReaderClosed();
            return _pos == _length ? -1 : _s[_pos];
        }
        return _pos == _length ? -1 : _s[_pos];
    }

    // Reads the next character from the underlying string. The returned value
    // is -1 if no further characters are available.
    //
    public override int Read()
    {
        if (_s is null)
        {
            ThrowReaderClosed();
        }

        return _pos == _length ? -1 : _s[_pos++];
    }

    // Reads a block of characters. This method will read up to count
    // characters from this StringReader into the buffer character
    // array starting at position index. Returns the actual number of
    // characters read, or zero if the end of the string is reached.
    //
    public override int Read([In, Out] char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (buffer.Length - index < count)
        {
            throw new ArgumentException("Invalid");
        }

        Contract.EndContractBlock();

        if (_s is null)
        {
            ThrowReaderClosed();
        }

        int n = _length - _pos;
        if (n > 0)
        {
            if (n > count) n = count;
            _s.CopyTo(_pos, buffer, index, n);
            _pos += n;
        }
        return n;
    }

    public override string ReadToEnd()
    {
        if (_s is null)
        {
            ThrowReaderClosed();
        }

        string s;
        if (_pos == 0)
        {
            s = _s;
        }
        else
        {
            s = _s[_pos.._length];
        }
        _pos = _length;
        return s;
    }

    // Reads a line. A line is defined as a sequence of characters followed by
    // a carriage return ('\r'), a line feed ('\n'), or a carriage return
    // immediately followed by a line feed. The resulting string does not
    // contain the terminating carriage return and/or line feed. The returned
    // value is null if the end of the underlying string has been reached.
    //
    public override string? ReadLine()
    {
        if (_s is null)
        {
            ThrowReaderClosed();
        }

        int i = _pos;
        while (i < _length)
        {
            char ch = _s[i];
            if (ch == '\r' || ch == '\n')
            {
                string result = _s.Substring(_pos, i - _pos);
                _pos = i + 1;
                if (ch == '\r' && _pos < _length && _s[_pos] == '\n') _pos++;
                return result;
            }
            i++;
        }
        if (i > _pos)
        {
            string result = _s.Substring(_pos, i - _pos);
            _pos = i;
            return result;
        }
        return null;
    }

    [DoesNotReturn]
    private static void ThrowReaderClosed()
    {
        throw new ObjectDisposedException(null, "ReusableStringReader is disposed.");
    }

    #region Task based Async APIs

    [ComVisible(false)]
    public override Task<string?> ReadLineAsync()
    {
        return Task.FromResult(ReadLine());
    }

    [ComVisible(false)]
    public override Task<String> ReadToEndAsync()
    {
        return Task.FromResult(ReadToEnd());
    }

    [ComVisible(false)]
    public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        if (index < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"));
        }
        if (buffer.Length - index < count)
        {
            throw new ArgumentException("Invalid Offset");
        }

        Contract.EndContractBlock();

        return Task.FromResult(ReadBlock(buffer, index, count));
    }

    [ComVisible(false)]
    public override Task<int> ReadAsync(char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
        if (index < 0 || count < 0)
        {
            throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"));
        }
        if (buffer.Length - index < count)
        {
            throw new ArgumentException("Invalid Offset");
        }

        Contract.EndContractBlock();

        return Task.FromResult(Read(buffer, index, count));
    }

    #endregion
}

