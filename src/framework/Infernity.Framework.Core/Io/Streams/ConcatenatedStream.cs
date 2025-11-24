namespace Infernity.Framework.Core.Io.Streams;

public class ConcatenatedStream : Stream
{
    public override bool CanRead { get; }
    public override bool CanSeek { get; }
    public override bool CanWrite { get; }
    public override long Length { get; }
    public override long Position { get; set; }

    public override int Read(byte[] buffer,
        int offset,
        int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset,
        SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer,
        int offset,
        int count)
    {
        throw new NotImplementedException();
    }
    
    public override void Flush()
    {
        throw new NotImplementedException();
    }

   
}