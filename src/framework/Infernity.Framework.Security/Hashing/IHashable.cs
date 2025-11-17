namespace Infernity.Framework.Security.Hashing;

public interface IHashable<T>
    where T : struct, IHashValue<T>
{
    void WriteHashData(in HashBuilder<T> builder);
}