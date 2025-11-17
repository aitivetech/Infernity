namespace Infernity.Framework.Core.Content;

public sealed record MimeType(
    string Id, 
    IReadOnlyList<string> Extensions,
    MimeTypeEncoding Encoding,
    MimeTypeCategory Category)
{
    public bool Is(string id)
        => Id == id;
    
    public bool Equals(MimeType? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return Id;
    }
}