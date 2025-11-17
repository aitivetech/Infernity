namespace Infernity.Framework.Security.Hashing;

public interface IHashProvider<T>
    where T : struct, IHashValue<T>
{
    HashBuilder<T> CreateBuilder();
}