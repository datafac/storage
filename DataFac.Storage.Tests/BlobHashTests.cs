using DataFac.Memory;
using Shouldly;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

namespace DataFac.Storage.Tests;

public class BlobHashTests
{
    [Fact]
    public void BlobHash01Empty()
    {
        ReadOnlySequence<byte> orig = default;
        BlobIdV1 id = orig.GetBlobId();
        id.IsEmbedded.ShouldBeTrue();
        id.ToString().ShouldBe("U:0:");
    }

    [Fact]
    public void BlobHash02NonEmpty()
    {
        var orig = new ReadOnlySequence<byte>(Enumerable.Range(0, 256).Select(i => (byte)i).ToArray());
        BlobIdV1 id = orig.GetBlobId();
        id.IsEmbedded.ShouldBeFalse();
        id.ToString().ShouldBe("V1.0:256:U:0:1:QK/y6dLYki5Hr9RkjmlnSXFYeF+9Hahw5xECZr+USIA=");
    }
}

public class CompressionTests
{
    private static readonly string sourcePara1 =
        """
        Twas bryllyg, and þe slythy toves
        Did gyre and gymble in þe wabe:
        All mimsy were þe borogoves;
        And þe mome raths outgrabe.
        """;
    private static Octets octetsPara1 = new Octets(Encoding.UTF8.GetBytes(sourcePara1));

    private static readonly string sourcePara2 =
        """
        Don't think me unkind
        Words are hard to find
        They're only cheques I've left unsigned
        From the banks of chaos in my mind
        And when their eloquence escapes me
        Their logic ties me up and rapes me
        Do-do-do-do, do-da-da-da
        Is all I want to say to you
        Do-do-do-do, do-da-da-da
        Their innocence will pull me through
        """;
    private static Octets octetsPara2 = new Octets(Encoding.UTF8.GetBytes(sourcePara2));

    private static Octets originalData = Octets.Combine(octetsPara1, octetsPara2);

    [Theory]
    [InlineData(BlobCompAlgo.UnComp, 0, (byte)'U')]
    [InlineData(BlobCompAlgo.Brotli, 1, (byte)'B')]
    [InlineData(BlobCompAlgo.Snappy, 2, (byte)'S')]
    public void RoundtripBlobCompAlgos(BlobCompAlgo algo, byte expectedByteValue, byte expectedCharCode)
    {
        byte byteValue = (byte)algo;
        byteValue.ShouldBe(expectedByteValue);

        byte charCode = algo.ToCharCode();
        charCode.ShouldBe(expectedCharCode);
        charCode.ToCompAlgo().ShouldBe(algo);
    }

    [Fact]
    public void RoundtripViaSnappier()
    {
        var compressedBuffer = new TestBufferWriter();
        Snappier.Snappy.Compress(originalData.Sequence, compressedBuffer);
        var compressed = compressedBuffer.GetWrittenSequence();

        var decompressedBuffer = new TestBufferWriter();
        Snappier.Snappy.Decompress(compressed, decompressedBuffer);

        var decompressed = Octets.UnsafeWrap(decompressedBuffer.GetWrittenSequence());
        decompressed.Equals(originalData).ShouldBeTrue();
    }

}

internal class TestBufferWriter : IBufferWriter<byte>
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