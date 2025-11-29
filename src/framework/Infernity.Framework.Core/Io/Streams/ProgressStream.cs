namespace Infernity.Framework.Core.Io.Streams;

public delegate void StreamProgressHandler(Stream stream,long total,int current);

public class ProgressStream : Stream
{
    private readonly Stream _innerStream;
    private readonly StreamProgressHandler _onStream;
    private readonly bool _leaveOpen;

    private long _processedBytes;
    
    public ProgressStream(Stream innerStream, StreamProgressHandler onStream,bool leaveOpen = false)
    {
        _innerStream = innerStream;
        _onStream = onStream;
        _leaveOpen = leaveOpen;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite =>  _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }
    
    public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

    public override void SetLength(long value) => _innerStream.SetLength(value);
    
    public override void Flush() => _innerStream.Flush();
    

    public override void Write(byte[] buffer,int offset,int count)
    {
        _innerStream.Write(buffer, offset, count);
        _processedBytes += count;
        _onStream(_innerStream, _processedBytes,count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _innerStream.Write(buffer);
        _processedBytes += buffer.Length;
        _onStream(_innerStream, _processedBytes,buffer.Length);
    }

    public override async Task WriteAsync(byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken)
    {
        await  _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        _processedBytes += count;
        _onStream(_innerStream, _processedBytes,count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await _innerStream.WriteAsync(buffer, cancellationToken);
        _processedBytes += buffer.Length;
        _onStream(_innerStream, _processedBytes,buffer.Length);
    }

    public override void WriteByte(byte value)
    {
        _innerStream.WriteByte(value);
        _processedBytes++;
        _onStream(_innerStream, _processedBytes,1);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = _innerStream.Read(buffer, offset, count);
        _processedBytes  += bytesRead;
        _onStream(_innerStream, _processedBytes,bytesRead);
        return bytesRead;
    }

    public override int Read(Span<byte> buffer)
    {
        int bytesRead = _innerStream.Read(buffer);
        _processedBytes += (long)bytesRead;
        _onStream(_innerStream, _processedBytes,bytesRead);
        return bytesRead;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        _processedBytes += bytesRead;
        _onStream(_innerStream,_processedBytes, bytesRead);
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int bytesRead = await _innerStream.ReadAsync(buffer, cancellationToken);
        _processedBytes += bytesRead;
        _onStream(_innerStream,_processedBytes, bytesRead);
        return bytesRead;
    }

    public override int ReadByte()
    {
        int result = _innerStream.ReadByte();
        _processedBytes += result >= 0 ? 1 : 0;
        _onStream(_innerStream,_processedBytes, result >= 0 ? 1 : 0);
        return result;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
            {
                _innerStream.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
        {
            await _innerStream.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}