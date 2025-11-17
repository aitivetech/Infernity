namespace Infernity.Framework.Core.Functional;

public readonly struct Unit
{
    public static readonly Unit Value = new();
    
    public bool Equals(Unit other)
    {
        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Unit other && Equals(other);
    }

    public override int GetHashCode()
    {
        return 0;
    }
}