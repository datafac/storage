using DataFac.Memory;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace DataFac.Storage;

public class CompressionBuffers: IBufferWriter<byte>
{
    private const int minSegmentSize = 16;
    private const int maxSegmentSize = 16 * 1024;

    private readonly LinkedList<ReadOnlyMemory<byte>> _savedBuffers = new LinkedList<ReadOnlyMemory<byte>>();
    private Memory<byte> _currentBuffer = Memory<byte>.Empty;
    private int _currentPosition = 0;

    public ReadOnlySequence<byte> GetWrittenSequence()
    {
        ReadOnlySequence<byte> result;
        if (_savedBuffers.Count == 0)
        {
            result = _currentPosition == 0
                ? ReadOnlySequence<byte>.Empty
                : new ReadOnlySequence<byte>(_currentBuffer.Slice(0, _currentPosition));
        }
        else
        {
            if (_currentPosition == 0)
            {
                if (_savedBuffers.Count == 1)
                {
                    result = new ReadOnlySequence<byte>(_savedBuffers.First());
                }
                else
                {
                    var builder = new ReadOnlySequenceBuilder<byte>(_savedBuffers);
                    result = builder.Build();
                }
            }
            else
            {
                var builder = new ReadOnlySequenceBuilder<byte>(_savedBuffers);
                builder = builder.Append(_currentBuffer.Slice(0, _currentPosition));
                result = builder.Build();
            }
        }

        // reset to intial state
        _savedBuffers.Clear();
        _currentBuffer = Memory<byte>.Empty;
        _currentPosition = 0;

        return result;
    }

    public void Advance(int count)
    {
        _currentPosition += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (sizeHint <= 0)
        {
            sizeHint = minSegmentSize;
        }

        // check remaining space in current buffer
        if ((_currentBuffer.Length - _currentPosition) >= sizeHint)
        {
            return _currentBuffer.Slice(_currentPosition);
        }

        // save current buffer
        if (_currentPosition > 0)
        {
            _savedBuffers.AddLast(_currentBuffer.Slice(0, _currentPosition));
            _currentBuffer = Memory<byte>.Empty;
            _currentPosition = 0;
        }

        // allocate next buffer
        int segmentSize;
        if (sizeHint > maxSegmentSize)
        {
            segmentSize = maxSegmentSize;
        }
        else
        {
            segmentSize = minSegmentSize;
            while (segmentSize < sizeHint && segmentSize < maxSegmentSize)
            {
                segmentSize *= 2;
            }
        }

        _currentBuffer = new byte[segmentSize];
        _currentPosition = 0;
        return _currentBuffer;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return GetMemory(sizeHint).Span;
    }
}
